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
uniform float color_wave_speed = 0.2;      // 颜色波动速度
uniform float color_wave_frequency = 2.0;  // 颜色波动频率
uniform float color_wave_amplitude = 0.3;  // 颜色波动幅度
uniform float hue_shift_speed = 0.1;       // 色相偏移速度
uniform float color_pulse_speed = 0.5;     // 颜色脉动速度
uniform float saturation_variation = 0.2;  // 饱和度变化

// 颜色参数
uniform vec4 color1 : source_color = vec4(0.1, 0.8, 0.5, 1.0);    // 翠绿
uniform vec4 color2 : source_color = vec4(0.3, 0.2, 0.8, 1.0);    // 蓝紫
uniform vec4 color3 : source_color = vec4(0.8, 0.3, 0.9, 1.0);    // 紫红
uniform vec4 glow_color : source_color = vec4(0.5, 1.0, 1.0, 1.0); // 辉光颜色

// 采样噪声并转换为 -1 到 1 的范围
float sample_noise(vec2 uv, float time_offset) {
    // 使用多个八度的噪声
    float noise = 0.0;
    float amplitude = 1.0;
    float frequency = 1.0;
    
    for (int i = 0; i < 4; i++) {
        vec2 sample_uv = uv * frequency * noise_scale + vec2(time_offset * 0.3, time_offset * 0.5);
        float n = texture(noise_tex, sample_uv).r * 2.0 - 1.0;
        noise += n * amplitude;
        
        frequency *= 2.0;
        amplitude *= 0.5;
    }
    
    return noise;
}

// 获取流动噪声
float get_flow_noise(vec2 uv, float time) {
    // 主要流动方向
    float flow1 = sample_noise(uv + vec2(time * 0.2, time * 0.1), 0.0);
    
    // 次要流动方向
    float flow2 = sample_noise(uv * 1.5 + vec2(time * 0.15, time * 0.25), 10.0);
    
    // 细节噪声
    float detail = sample_noise(uv * 3.0 + vec2(time * 0.4, 0.0), 20.0) * 0.3;
    
    return flow1 * 0.6 + flow2 * 0.3 + detail;
}

// RGB转HSV
vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

// HSV转RGB
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// 生成颜色波动
vec3 apply_color_wave(vec3 color, vec2 uv, float time, float layer_index) {
    // 将RGB转换为HSV，便于调整色相
    vec3 hsv = rgb2hsv(color);
    
    // 色相偏移 - 波浪效果
    float hue_wave = sin(uv.x * color_wave_frequency + 
                        uv.y * color_wave_frequency * 0.7 + 
                        time * hue_shift_speed + 
                        layer_index * 2.094) * color_wave_amplitude;
    
    // 饱和度波动
    float saturation_wave = sin(uv.y * color_wave_frequency * 1.5 + 
                               time * color_wave_speed * 1.2 + 
                               layer_index * 1.57) * saturation_variation * 0.5 + 0.5;
    
    // 亮度脉动
    float value_wave = sin(time * color_pulse_speed + 
                          uv.x * 2.0 + 
                          layer_index * 3.14159) * 0.1 + 0.9;
    
    // 应用颜色波动
    hsv.x = mod(hsv.x + hue_wave * 0.2, 1.0);  // 色相波动（限制在0-1范围内）
    hsv.y = clamp(hsv.y * (0.8 + saturation_wave * 0.4), 0.3, 1.0);  // 饱和度波动
    hsv.z = hsv.z * value_wave;  // 亮度波动
    
    // 转回RGB
    return hsv2rgb(hsv);
}

void fragment() {
    // 调整UV，保持宽高比
    vec2 uv = UV;
    float aspect_ratio = SCREEN_PIXEL_SIZE.x / SCREEN_PIXEL_SIZE.y;
    vec2 scaled_uv = (uv - 0.5) * vec2(aspect_ratio, 1.0) * 2.0;
    
    // 垂直波浪
    float vertical_waves = sin(TIME * 0.2);
    
    float t = TIME * time_scale;
    
    float aurora = 0.0;
    vec3 color_accum = vec3(0.0);
    float intensity_accum = 0.0;
    
    // 多层极光带
    for (float i = 0.0; i < band_count; i++) {
        float layer_t = i / band_count;
        float layer_speed = speed * (0.8 + layer_t * 0.4);
        
        // 基础波浪形状
        float wave = sin(scaled_uv.x * (4.0 + i * 1.5) + t * layer_speed * 1.2 + layer_t * 5.0) * 0.5;
        wave += sin(scaled_uv.x * (2.0 + i) + t * layer_speed * 0.8 + layer_t * 3.0) * 0.3;
        
        // 垂直方向的波浪
        float vertical = sin(scaled_uv.y * (6.0 - i * 0.5) + t * layer_speed * 0.6) * vertical_waves;
        
        // 使用噪声纹理扭曲UV
        vec2 distorted_uv = uv;
        distorted_uv.x += sample_noise(uv * 2.0 + vec2(t * 0.1, 0.0), i * 10.0) * distortion_strength * 0.1;
        distorted_uv.y += sample_noise(uv * 1.5 + vec2(0.0, t * 0.2), i * 20.0) * distortion_strength * 0.05;
        
        // 获取流动噪声
        float flow_noise = get_flow_noise(distorted_uv * 1.5 + vec2(t * 0.15, t * 0.1), t);
        
        // 组合形状
        float shape = uv.y - wave - vertical + flow_noise * 0.4;
        
        // 创建极光带
        float band_thickness = 0.12 + layer_t * 0.05;
        float band = smoothstep(band_thickness, 0.0, abs(shape));
        
        // 添加渐变
        band *= 1.0 - smoothstep(0.0, band_thickness * 2.0, abs(shape));
        
        // 层间衰减
        float layer_fade = 1.0 - layer_t * 0.5;
        band *= layer_fade;
        
        // 颜色波动的强度调制
        float color_intensity_mod = 0.8 + 0.2 * sin(t * color_pulse_speed + i * 2.0);
        band *= color_intensity_mod;
        
        aurora += band;
        
        // 动态颜色混合 - 波浪效果
        float color_mix1 = sin(scaled_uv.x * 1.5 + t * color_wave_speed + i * 2.094) * 0.5 + 0.5;
        float color_mix2 = cos(scaled_uv.y * 2.0 + t * color_wave_speed * 1.3 + i * 1.047) * 0.5 + 0.5;
        
        // 添加颜色流动
        float color_flow = sin(uv.x * color_wave_frequency + t * color_wave_speed * 2.0 + i * 3.14159) * 0.5 + 0.5;
        color_mix1 = mix(color_mix1, color_flow, 0.3);
        
        // 三层颜色混合
        vec3 color_a = mix(color1.rgb, color2.rgb, color_mix1);
        vec3 color_b = mix(color_a, color3.rgb, color_mix2);
        
        // 基于高度调整颜色
        float height_factor = smoothstep(0.3, 0.7, uv.y);
        vec3 base_color = mix(color_b, glow_color.rgb, height_factor * 0.3);
        
        // 应用颜色波动
        base_color = apply_color_wave(base_color, uv, t, i);
        
        // 添加噪声细节
        float color_noise = sample_noise(uv * 3.0 + vec2(t * 0.1, t * 0.05), 30.0 + i * 50.0) * 0.2 + 0.9;
        base_color *= color_noise;
        
        // 添加颜色闪烁
        float color_flicker = 0.9 + 0.1 * sin(t * 3.0 + uv.x * 5.0 + i * 2.0);
        base_color *= color_flicker;
        
        // 累加颜色和强度
        color_accum += base_color * band;
        intensity_accum += band;
    }
    
    // 平均颜色
    vec3 avg_color = color_accum / max(1.0, intensity_accum);
    
    aurora = clamp(aurora * intensity, 0.0, 1.0);
    
    // 全局颜色调制
    float global_color_wave = sin(t * hue_shift_speed) * 0.1 + 0.9;
    vec3 final_base_color = avg_color * global_color_wave;
    
    // 添加全局颜色脉冲
    float global_pulse = sin(t * color_pulse_speed) * 0.05 + 1.0;
    final_base_color *= global_pulse;
    
    // 辉光效果
    float glow = smoothstep(0.1, 0.4, aurora);
    glow *= glow_intensity;
    
    // 边缘发光
    float edge_glow = smoothstep(0.4, 0.1, aurora);
    glow += edge_glow * glow_intensity * 0.3;
    
    // 最终颜色合成
    vec3 final_color = final_base_color * aurora;
    
    // 辉光颜色也波动
    vec3 glowing_glow_color = apply_color_wave(glow_color.rgb, uv, t * 1.5, 0.0);
    final_color += glowing_glow_color * glow * 2.0;
    
    // 添加星光效果
    vec2 star_uv = uv * 100.0;
    float stars = texture(noise_tex, star_uv + vec2(t * 0.02, 0.0)).r;
    stars = pow(stars, 8.0) * 0.5;
    stars *= 1.0 - smoothstep(0.3, 0.8, aurora); // 在极光弱的地方星星更亮
    
    // 星星颜色也轻微波动
    float star_color_wave = sin(t * 0.5 + uv.x * 3.0) * 0.1 + 0.9;
    final_color += vec3(1.0, 0.9, 0.8) * stars * 0.15 * star_color_wave;
    
    // 动态亮度变化
    float brightness_variation = sin(t * 0.2) * 0.1 + 0.9;
    final_color *= brightness_variation;
    
    // 添加颜色呼吸效果
    float color_breathing = sin(t * 0.3) * 0.05 + 1.0;
    final_color *= color_breathing;
    
    // 输出
    float alpha = aurora * 0.8 + glow * 0.2;
    COLOR = vec4(final_color, alpha);
}