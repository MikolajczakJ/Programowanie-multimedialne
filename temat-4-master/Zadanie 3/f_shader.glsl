#version 330



out vec4 pixelColor; //Zmienna wyjsciowa fragment shadera. Zapisuje sie do niej ostateczny (prawie) kolor piksela

in vec4 i_c;
in vec4 v_c; 
in vec4 n_c;
uniform mat4 P;
uniform mat4 V;
uniform mat4 M;

void main(void) {

	
	pixelColor=i_c;
	 vec4 polZroSwi=vec4(0,0,-6,1);
    vec4 l = normalize(V*polZroSwi-V*M*vertex);
    vec4 n = normalize(V*M*normal);
    vec4 r=reflect(-l,n);
    vec4 v=normalize(vec4(0,0,0,1)-V*M*vertex);
    float rv=clamp(dot(r,v),0,1);
    
    rv=pow(rv,1);
    gl_Position=P*V*M*vertex;
 
 i_c = color*vec4(0,0,0,1) + color*vec4(1,1,1,1) *dot(n,l)+color*color*rv;
 pivelColor = i_c;


}
