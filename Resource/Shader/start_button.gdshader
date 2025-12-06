shader_type canvas_item;

uniform float offset : hint_range(-1.0, 1.0) = 0.0;
uniform float rotation : hint_range(-360.0,360.0) = 0.0;
uniform float alpha : hint_range(0.0, 1.0) = 1.0;
uniform sampler2D noise_tex : source_color, repeat_enable;

uniform float dissolve_progress : hint_range(0.0, 2.0) = 0.0;
uniform sampler2D noise_tex2 : source_color;

uniform float edge_position = 0.02;
uniform float edge_width = 0.04; 


vec2 rotated(vec2 source, float angle, vec2 center){
	float c = cos(rotation / 180.0 * PI);
	float s = sin(rotation / 180.0 * PI);
	vec2 pos = source - center;
	vec2 rotated = vec2(
		pos.x * c - pos.y * s,
		pos.x * s + pos.y * c
	);
	return rotated;
}

void vertex(){

	vec2 center = 0.5 / TEXTURE_PIXEL_SIZE;
	
	VERTEX = rotated(VERTEX,rotation,center) + center;
	VERTEX.x += offset * 100.0;
}
void fragment(){
	COLOR.a = alpha;
	float noise = texture(noise_tex2, UV * vec2(1,1)).r;
	float dissolve_threshold = dissolve_progress - noise * 1.0;
	float is_dissolved = step(UV.x, dissolve_threshold);
	float final_alpha = COLOR.a * (alpha - is_dissolved);
	COLOR.a = final_alpha;
	if(UV.x > 1.0 - edge_position - edge_width && UV.x < 1.0 - edge_position && UV.y < 1.0 - edge_position && UV.y > edge_position ||
   UV.x > edge_position && UV.x < 1.0 - edge_position && UV.y > 1.0 - edge_position - edge_width && UV.y < 1.0 - edge_position ||
   UV.x < edge_position + edge_width && UV.x > edge_position && UV.y < 1.0 - edge_position && UV.y > edge_position ||
   UV.x > edge_position && UV.x < 1.0 - edge_position && UV.y > edge_position && UV.y < edge_position + edge_width){
   COLOR.a = 0.0;
}

	vec2 uv = UV;
    float t = TIME * 0.3;
    
    float total_aurora = 0.0;
    vec3 total_color = vec3(0.0);
    
    // 3层极光
    for(int i = 0; i < 6; i++) {
        float layer_t = float(i) * 8.0;
        
        // 每层不同的波浪
        float wave = sin(uv.x * (0.1 + float(i)) + t + layer_t * 9.0) * sin(TIME * 0.1) * 0.7;
        float aurora = smoothstep(0.3 + float(i)*0.03, 0.0, 
                                 abs(uv.y - 0.5 - wave - float(i)*0.05));
        aurora *= (1.0 - uv.y * 0.7) * (1.0 - float(i)*0.4); // 渐变和层衰减
        
        // 每层不同的噪声
        float noise = texture(noise_tex, uv * (2.0 + float(i)*0.5) + 
                             vec2(t * (0.1 + float(i)*0.05), 0.0)).r;
        aurora *= 0.6 + noise * 0.4;
        
        // 每层不同的颜色
        vec3 layer_color = mix(
            mix(vec3(0.1, 0.8, 0.5), vec3(0.3, 0.2, 0.8), float(i)/2.0),
            vec3(0.8, 0.3, 0.9),
            sin(uv.x * 1.5 + t * 0.3 + layer_t) * 0.5 + 0.5
        );
        
        total_aurora += aurora;
        total_color += layer_color * aurora;
    }
    
    // 平均颜色
    vec3 aurora_color = total_color / max(1.0, total_aurora);
    
    // 叠加到原有颜色
    COLOR.rgb = mix(COLOR.rgb, aurora_color, total_aurora * 0.8);
	COLOR.a = min(COLOR.t * 4.0 - 3.0,COLOR.a);
}