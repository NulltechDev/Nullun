shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;

void fragment() {
    // 获取屏幕纹理的颜色
    vec4 screen_color = texture(screen_texture, SCREEN_UV);
    
    // 获取TextureRect自身的纹理
    vec4 mask_color = texture(TEXTURE, UV);
	COLOR = vec4(COLOR.a);
    
    // 如果纹理有像素（alpha > 0），就反色；否则不反色
    // 完全不使用纹理的颜色信息，只看alpha通道
    if (mask_color.a > 0.0) {
        // 完全反色
        COLOR = vec4(1.0 - screen_color.r, 1.0 - screen_color.g, 1.0 - screen_color.b, screen_color.a) * COLOR;
    } else {
        // 不反色
        COLOR = screen_color * COLOR;
    }
}