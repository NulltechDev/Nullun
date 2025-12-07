shader_type canvas_item;

// 保守参数：先保证图像显示，再微调
uniform int blur_radius : hint_range(1, 10) = 5;        // 降低模糊半径，避免采样过度
uniform float blur_strength : hint_range(0.5, 1.5) = 1.0; // 降低强度
uniform float edge_softness : hint_range(0.0, 2.0) = 1.0; // 弱化衰减
uniform float corner_fix : hint_range(0.0, 1.0) = 0.5;   // 四角修复强度（0=关闭修复，先设0.5）
uniform float alpha_min : hint_range(0.0, 1.0) = 0.1;    // 强制保留最低Alpha，避免消失
uniform vec4 color : source_color;

uniform float inner_fade_width : hint_range(0.0, 0.5) = 0.2; // 向内过渡的宽度（0.2=20%纹理宽度）
uniform float inner_fade_softness : hint_range(0.0, 5.0) = 2.0; // 过渡柔和度
uniform float min_alpha : hint_range(0.0, 1.0) = 0.9;   // 中心区域最低Alpha（保证主体不消失）

// 高斯核（保留模糊）
float gaussian(float x, float y, float sigma) {
    return max(exp(-(x*x + y*y) / (2.0 * sigma*sigma)) / (2.0 * PI * sigma*sigma), 0.01);
}

void fragment() {
    vec2 uv = UV;
    vec2 tex_pixel = TEXTURE_PIXEL_SIZE;
    vec3 final_rgb = vec3(0.0);
    float final_alpha = 0.0;
    float total_weight = 0.0;
    float sigma = float(blur_radius) * 0.35 * blur_strength;

    // 高斯模糊采样（仅纹理内，保证不消失）
    for (int x = -blur_radius; x <= blur_radius; x++) {
        for (int y = -blur_radius; y <= blur_radius; y++) {
            vec2 offset = vec2(float(x) * tex_pixel.x, float(y) * tex_pixel.y);
            vec2 sample_uv = clamp(uv + offset, vec2(0.0), vec2(1.0)); // 限制在纹理内
            vec4 tex_color = texture(TEXTURE, sample_uv);

            // 高斯权重（保底，避免归0）
            float gauss_weight = gaussian(float(x), float(y), sigma);
            total_weight += gauss_weight;

            // 累加模糊颜色
            final_rgb += tex_color.rgb * gauss_weight;
            final_alpha += tex_color.a * gauss_weight;
        }
    }

    // 归一化模糊结果
    final_rgb = total_weight > 0.0 ? final_rgb / total_weight : texture(TEXTURE, uv).rgb;
    final_alpha = total_weight > 0.0 ? final_alpha / total_weight : texture(TEXTURE, uv).a;

    // 核心：朝内透明过渡逻辑
    // 1. 计算当前UV到「纹理边缘向内偏移inner_fade_width」的距离
    float dist_to_inner_edge = min(
        min(uv.x - inner_fade_width, 1.0 - uv.x - inner_fade_width),
        min(uv.y - inner_fade_width, 1.0 - uv.y - inner_fade_width)
    );
    // 2. 反转衰减：边缘（dist_to_inner_edge < 0）Alpha=0，向内逐渐升到min_alpha
    float inner_fade = smoothstep(-inner_fade_width, 0.0, dist_to_inner_edge) * inner_fade_softness;
    inner_fade = clamp(inner_fade, 0.0, 1.0); // 限制在0~1

    // 3. 最终Alpha：朝内过渡 + 保留中心最低Alpha
    final_alpha = final_alpha * inner_fade;
    final_alpha = max(final_alpha, min_alpha * inner_fade); // 中心区域不透明

    // 输出：朝内过渡+高斯模糊
    COLOR = vec4(final_rgb, final_alpha * COLOR.a) * color;
}