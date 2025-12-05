shader_type canvas_item;

uniform float offset : hint_range(-1.0, 1.0) = 0.0;
uniform float rotation : hint_range(-360.0,360.0) = 0.0;
uniform float alpha : hint_range(0.0, 1.0) = 1.0;

void vertex(){

	vec2 center = 0.5 / TEXTURE_PIXEL_SIZE;
	float c = cos(rotation / 180.0 * PI);
	float s = sin(rotation / 180.0 * PI);
	vec2 pos = VERTEX - center;
	vec2 rotated = vec2(
		pos.x * c - pos.y * s,
		pos.x * s + pos.y * c
	);
	VERTEX = rotated + center;
	VERTEX.x += offset * 100.0;
}
void fragment(){
	COLOR.a = alpha;
}