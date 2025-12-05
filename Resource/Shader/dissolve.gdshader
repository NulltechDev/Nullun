shader_type canvas_item;

uniform float dissolve_progress : hint_range(0.0, 2.0) = 0.0;
uniform float alpha : hint_range(0.0, 1.0) = 1.0;
uniform sampler2D noise_tex : source_color;

void fragment() {
    vec4 original_color = texture(TEXTURE, UV);

    float noise = texture(noise_tex, UV * vec2(1,1)).r;
    
    float dissolve_threshold = dissolve_progress - noise * 1.0;
    float is_dissolved = step(UV.x, dissolve_threshold);

    float final_alpha = original_color.a * (alpha - is_dissolved);
    
    COLOR = vec4(original_color.rgb, final_alpha);
}