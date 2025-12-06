shader_type canvas_item;

uniform float alpha : hint_range(0.0, 1.0) = 1.0;
uniform sampler2D noise_tex : source_color, filter_linear;

void fragment() {
    vec4 original_color = texture(TEXTURE, UV);

    float noise = texture(noise_tex, UV).r;
    float dissolve_threshold = 1.5 - noise;
    float is_dissolved = smoothstep(dissolve_threshold - 0.02, dissolve_threshold + 0.02, UV.x);

    float final_alpha = original_color.a * (alpha - is_dissolved);

    COLOR = vec4(original_color.rgb, final_alpha);
}