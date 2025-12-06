shader_type canvas_item;
uniform sampler2D noise_tex : source_color, repeat_enable;
uniform vec4 glitch_color : source_color = vec4(0.0, 0.0, 0.0, 1.0); // 基础故障色（默认黑色）

void fragment() {
    float base_noise = texture(noise_tex, UV).r;
    float noise2 = texture(noise_tex, UV + vec2(0.5, 0.2)).r;
    
    vec2 noise_uv = UV * (0.6 + base_noise * 0.8) + vec2(noise2, base_noise) * 0.2;
    float noise = texture(noise_tex, noise_uv).r;
    float time_offset = TIME * 5.0 * noise;
    float dynamic_noise = texture(noise_tex, UV + time_offset * 0.1).r;

    // 纯颜色驱动故障（无原纹理）
    COLOR.rgb = glitch_color.rbg; // 基础底色
	COLOR.a *= glitch_color.a;
    COLOR *= step(0.45, dynamic_noise);
}