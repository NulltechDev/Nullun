shader_type canvas_item;

// 噪声纹理
uniform sampler2D noise_tex : source_color, repeat_enable, filter_linear_mipmap;

// 控制参数
uniform float time_scale = 1.0;
uniform float speed = 0.3;
uniform float intensity = 1.0;
uniform float noise_scale = 2.0;
uniform float band_count = 4.0;
uniform float glow_intensity = 0.2;
uniform float distortion_strength = 0.0;

// 颜色波动参数
uniform float color_wave_speed = 0.2;
uniform float color_wave_frequency = 2.0;
uniform float color_wave_amplitude = 0.3;
uniform float hue_shift_speed = 0.1;
uniform float color_pulse_speed = 0.5;
uniform float saturation_variation = 0.2;

// 颜色参数
uniform vec4 color1 : source_color = vec4(0.1, 0.8, 0.5, 1.0);
uniform vec4 color2 : source_color = vec4(0.3, 0.2, 0.8, 1.0);
uniform vec4 color3 : source_color = vec4(0.8, 0.3, 0.9, 1.0);
uniform vec4 glow_color : source_color = vec4(0.5, 1.0, 1.0, 1.0);

// 采样噪声（优化版本）
float sample_noise(vec2 uv, float time_offset) {
    // 预计算缩放和时间偏移
    vec2 scaled_uv = uv * noise_scale;
    vec2 time_vec = vec2(time_offset * 0.3, time_offset * 0.5);
    
    // 使用4个八度，但减少纹理采样次数
    float noise = 0.0;
    float amplitude = 0.5;
    vec2 current_uv = scaled_uv + time_vec;
    
    // 第一层
    float n1 = texture(noise_tex, current_uv).r * 2.0 - 1.0;
    noise += n1 * amplitude;
    
    // 第二层
    amplitude *= 0.5;
    current_uv = scaled_uv * 2.0 + time_vec * 1.5;
    float n2 = texture(noise_tex, current_uv).r * 2.0 - 1.0;
    noise += n2 * amplitude;
    
    // 第三层
    amplitude *= 0.5;
    current_uv = scaled_uv * 4.0 + time_vec * 2.0;
    float n3 = texture(noise_tex, current_uv).r * 2.0 - 1.0;
    noise += n3 * amplitude;
    
    // 第四层
    amplitude *= 0.5;
    current_uv = scaled_uv * 8.0 + time_vec * 2.5;
    float n4 = texture(noise_tex, current_uv).r * 2.0 - 1.0;
    noise += n4 * amplitude;
    
    return noise;
}

// 获取流动噪声（优化版本）
float get_flow_noise(vec2 uv, float time, float layer_index) {
    // 合并时间计算
    float time_factor = time * 0.15;
    
    // 主要流动
    vec2 sample_uv1 = uv * 1.5 + vec2(time_factor * 0.2, time_factor * 0.1);
    float flow1 = sample_noise(sample_uv1, layer_index);
    
    // 次要流动（简化）
    vec2 sample_uv2 = uv * 2.25 + vec2(time_factor * 0.15, time_factor * 0.25);
    float flow2 = sample_noise(sample_uv2, layer_index + 10.0) * 0.5;
    
    return flow1 * 0.67 + flow2 * 0.33;
}

// RGB转HSV（优化版本）
vec3 rgb2hsv(vec3 c) {
    float r = c.r, g = c.g, b = c.b;
    float max_rgb = max(max(r, g), b);
    float min_rgb = min(min(r, g), b);
    float delta = max_rgb - min_rgb;
    
    float h = 0.0;
    if (delta > 0.0) {
        if (max_rgb == r) {
            h = (g - b) / delta;
        } else if (max_rgb == g) {
            h = 2.0 + (b - r) / delta;
        } else {
            h = 4.0 + (r - g) / delta;
        }
        h = mod(h / 6.0, 1.0);
    }
    
    float s = max_rgb > 0.0 ? delta / max_rgb : 0.0;
    
    return vec3(h, s, max_rgb);
}

// HSV转RGB（优化版本）
vec3 hsv2rgb(vec3 c) {
    float h = c.x * 6.0;
    float s = c.y;
    float v = c.z;
    
    int i = int(floor(h));
    float f = h - float(i);
    float p = v * (1.0 - s);
    float q = v * (1.0 - s * f);
    float t = v * (1.0 - s * (1.0 - f));
    
    if (i == 0) return vec3(v, t, p);
    else if (i == 1) return vec3(q, v, p);
    else if (i == 2) return vec3(p, v, t);
    else if (i == 3) return vec3(p, q, v);
    else if (i == 4) return vec3(t, p, v);
    else return vec3(v, p, q);
}

// 生成颜色波动（优化版本）
vec3 apply_color_wave(vec3 color, vec2 uv, float time, float layer_index) {
    vec3 hsv = rgb2hsv(color);
    
    // 合并计算
    float hue_wave_pos = uv.x * color_wave_frequency + uv.y * color_wave_frequency * 0.7 + 
                        time * hue_shift_speed + layer_index * 2.094;
    float hue_wave = sin(hue_wave_pos) * color_wave_amplitude;
    
    // 饱和度波动
    float sat_wave_pos = uv.y * color_wave_frequency * 1.5 + time * color_wave_speed * 1.2 + 
                        layer_index * 1.57;
    float saturation_wave = sin(sat_wave_pos) * saturation_variation * 0.5 + 0.5;
    
    // 亮度脉动
    float value_wave_pos = time * color_pulse_speed + uv.x * 2.0 + layer_index * 3.14159;
    float value_wave = sin(value_wave_pos) * 0.1 + 0.9;
    
    // 应用颜色波动
    hsv.x = mod(hsv.x + hue_wave * 0.2, 1.0);
    hsv.y = clamp(hsv.y * (0.8 + saturation_wave * 0.4), 0.3, 1.0);
    hsv.z = hsv.z * value_wave;
    
    return hsv2rgb(hsv);
}

void fragment() {
    // 计算基本时间变量
    float current_time = TIME * time_scale;
    float time_0_2 = current_time * 0.2;
    float time_0_3 = current_time * 0.3;
    float time_0_5 = current_time * 0.5;
    float time_1_5 = current_time * 1.5;
    float color_pulse_time = current_time * color_pulse_speed;
    float hue_shift_time = current_time * hue_shift_speed;
    
    // 调整UV，保持宽高比
    vec2 uv = UV;
    float aspect_ratio = SCREEN_PIXEL_SIZE.x / SCREEN_PIXEL_SIZE.y;
    vec2 scaled_uv = (uv - 0.5) * vec2(aspect_ratio, 1.0) * 2.0;
    
    // 垂直波浪
    float vertical_waves = sin(time_0_2);
    
    float aurora = 0.0;
    vec3 color_accum = vec3(0.0);
    float intensity_accum = 0.0;
    
    // 预计算循环常量
    float inv_band_count = 1.0 / band_count;
    
    // 多层极光带（优化循环）
    for (float i = 0.0; i < band_count; i += 1.0) {
        float layer_t = i * inv_band_count;
        float layer_speed = speed * (0.8 + layer_t * 0.4);
        float layer_time = current_time * layer_speed;
        
        // 预计算三角函数参数
        float wave_arg1 = scaled_uv.x * (4.0 + i * 1.5) + layer_time * 1.2 + i * 5.0;
        float wave_arg2 = scaled_uv.x * (2.0 + i) + layer_time * 0.8 + i * 3.0;
        
        // 基础波浪形状（合并计算）
        float wave = sin(wave_arg1) * 0.5 + sin(wave_arg2) * 0.3;
        
        // 垂直方向的波浪
        float vertical = sin(scaled_uv.y * (6.0 - i * 0.5) + layer_time * 0.6) * vertical_waves;
        
        // 获取流动噪声
        float flow_noise = get_flow_noise(uv * 1.5, current_time, i);
        
        // 组合形状
        float shape = uv.y - wave - vertical + flow_noise * 0.4;
        
        // 创建极光带（优化计算）
        float band_thickness = 0.12 + layer_t * 0.05;
        float abs_shape = abs(shape);
        
        // 使用线性插值替代smoothstep
        float band = clamp((band_thickness - abs_shape) / band_thickness, 0.0, 1.0);
        
        // 添加渐变
        float fade_range = band_thickness * 2.0;
        band *= clamp((fade_range - abs_shape) / fade_range, 0.0, 1.0);
        
        // 层间衰减
        float layer_fade = 1.0 - layer_t * 0.5;
        band *= layer_fade;
        
        // 颜色波动的强度调制
        float color_intensity_mod = 0.8 + 0.2 * sin(color_pulse_time + i * 2.0);
        band *= color_intensity_mod;
        
        aurora += band;
        
        // 动态颜色混合
        float color_mix1 = sin(scaled_uv.x * 1.5 + current_time * color_wave_speed + i * 2.094) * 0.5 + 0.5;
        float color_mix2 = cos(uv.y * 2.0 + current_time * color_wave_speed * 1.3 + i * 1.047) * 0.5 + 0.5;
        
        // 添加颜色流动
        float color_flow = sin(uv.x * color_wave_frequency + current_time * color_wave_speed * 2.0 + i * 3.14159) * 0.5 + 0.5;
        color_mix1 = mix(color_mix1, color_flow, 0.3);
        
        // 三层颜色混合
        vec3 color_a = mix(color1.rgb, color2.rgb, color_mix1);
        vec3 base_color = mix(color_a, color3.rgb, color_mix2);
        
        // 基于高度调整颜色
        float height_factor = clamp((uv.y - 0.3) / 0.4, 0.0, 1.0);
        base_color = mix(base_color, glow_color.rgb, height_factor * 0.3);
        
        // 应用颜色波动
        base_color = apply_color_wave(base_color, uv, current_time, i);
        
        // 添加噪声细节
        float color_noise = sample_noise(uv * 3.0, 30.0 + i * 50.0) * 0.2 + 0.9;
        base_color *= color_noise;
        
        // 添加颜色闪烁
        float color_flicker = 0.9 + 0.1 * sin(current_time * 3.0 + uv.x * 5.0 + i * 2.0);
        base_color *= color_flicker;
        
        // 累加颜色和强度
        color_accum += base_color * band;
        intensity_accum += band;
    }
    
    // 平均颜色
    vec3 avg_color = intensity_accum > 0.0 ? color_accum / intensity_accum : vec3(0.0);
    
    aurora = clamp(aurora * intensity, 0.0, 1.0);
    
    // 全局颜色调制
    float global_color_wave = sin(hue_shift_time) * 0.1 + 0.9;
    vec3 final_base_color = avg_color * global_color_wave;
    
    // 添加全局颜色脉冲
    float global_pulse = sin(color_pulse_time) * 0.05 + 1.0;
    final_base_color *= global_pulse;
    
    // 辉光效果（优化计算）
    float glow = clamp((aurora - 0.1) / 0.3, 0.0, 1.0);
    glow *= glow_intensity;
    
    // 边缘发光
    float edge_glow = clamp((0.4 - aurora) / 0.3, 0.0, 1.0);
    glow += edge_glow * glow_intensity * 0.3;
    
    // 最终颜色合成
    vec3 final_color = final_base_color * aurora;
    
    // 辉光颜色也波动
    vec3 glowing_glow_color = apply_color_wave(glow_color.rgb, uv, time_1_5, 0.0);
    final_color += glowing_glow_color * glow * 2.0;
    
    // 添加星光效果（优化计算）
    vec2 star_uv = uv * 100.0;
    float stars = texture(noise_tex, star_uv + vec2(current_time * 0.02, 0.0)).r;
    
    // 展开pow(stars, 8.0)计算
    stars = stars * stars; // stars^2
    stars = stars * stars; // stars^4
    stars = stars * stars; // stars^8
    stars *= 0.5;
    
    // 在极光弱的地方星星更亮
    stars *= clamp((0.3 - aurora) / 0.5, 0.0, 1.0);
    
    // 星星颜色也轻微波动
    float star_color_wave = sin(time_0_5 + uv.x * 3.0) * 0.1 + 0.9;
    final_color += vec3(1.0, 0.9, 0.8) * stars * 0.15 * star_color_wave;
    
    // 动态亮度变化
    float brightness_variation = sin(time_0_2) * 0.1 + 0.9;
    final_color *= brightness_variation;
    
    // 添加颜色呼吸效果
    float color_breathing = sin(time_0_3) * 0.05 + 1.0;
    final_color *= color_breathing;
    
    // 输出
    float alpha = aurora * 0.8 + glow * 0.2;
    COLOR = vec4(final_color, alpha);
}