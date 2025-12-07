shader_type canvas_item;

uniform float offset : hint_range(-1.0, 1.0) = 0.0;
uniform float rotation : hint_range(-360.0,360.0) = 0.0;
uniform float alpha : hint_range(0.0, 1.0) = 1.0;

uniform float dissolve_progress : hint_range(0.0, 2.0) = 0.0;
uniform sampler2D noise_tex : source_color;

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
	float noise = texture(noise_tex, UV * vec2(1,1)).r;
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
}