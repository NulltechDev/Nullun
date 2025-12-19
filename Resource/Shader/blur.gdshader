shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE: hint_screen_texture,repeat_disable, filter_nearest;
uniform float strength : hint_range(0.1, 20.0) = 3.3;
uniform float mix_percentage: hint_range(0.0, 1.0) = 0.3;

float gaussianDistribution(float x, float STD){
	return exp(-(x*x)/(2.*STD*STD))/(sqrt(2.*PI)*STD);
}

vec3 gaussianblur(sampler2D sampler, vec2 pos, vec2 pixel_size, float sigmaUsed, int radius){
    vec3 blurredPixel = vec3(0.0);
    float total_weight = 0.0;
    // 提前计算sigma相关常量，避免循环内重复计算
    float sigma2 = sigmaUsed * sigmaUsed;
    float inv_denom = 1.0 / (sqrt(2.0 * PI) * sigmaUsed);
    float inv_2sigma2 = 1.0 / (2.0 * sigma2);
    
    // 合并循环内重复计算：提前计算pixel_size的乘法、减少vec2构造
    vec2 pixel_step = pixel_size;
    for(int i = -radius ; i <= radius; i++){ 
        float fi = float(i);
        float weight_i = exp(-(fi*fi)*inv_2sigma2) * inv_denom;
        vec2 offset_i = fi * pixel_step;
        
        for(int j = -radius; j <= radius; j++){
            float fj = float(j);
            float weight = weight_i * exp(-(fj*fj)*inv_2sigma2) * inv_denom;
            
            blurredPixel += texture(SCREEN_TEXTURE, pos + vec2(fi, fj) * pixel_step).rgb * weight;
            total_weight += weight;
        }
    }
	blurredPixel /= total_weight;
    return blurredPixel;
}

void fragment() {
    // 提前计算半径，避免函数内重复计算
    int blur_radius = int(round(3.0 * strength));
    vec3 PixelBlurred = gaussianblur(SCREEN_TEXTURE, SCREEN_UV, SCREEN_PIXEL_SIZE, strength, blur_radius);
    // 简化mix计算（直接用乘法替代vec3构造+mix）
    vec3 color = PixelBlurred * (1.0 - mix_percentage);
    COLOR = vec4(color, 1.0);
}