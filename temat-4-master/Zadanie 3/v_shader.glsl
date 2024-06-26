#version 330

//Zmienne jednorodne
uniform mat4 P;
uniform mat4 V;
uniform mat4 M;

//Atrybuty
in vec4 vertex; //wspolrzedne wierzcholka w przestrzeni modelu
in vec4 normal; //wektor normalny w przestrzeni modelu
in vec4 color; //kolor skojarzony z wierzcho�kiem
in vec2 texCoord; //wsp�rz�dna teksturowana

out vec4 i_c;
out vec4 i_v;
out vec4 i_n;
void main(void) {
    i_c=color;
    i_v = vertex;
    i_n = normal;
    vec4 polZroSwi=vec4(0,0,-6,1);
    vec4 l = normalize(V*polZroSwi-V*M*vertex);
    vec4 n = normalize(V*M*normal);
    vec4 r=reflect(-l,n);
    vec4 v=normalize(vec4(0,0,0,1)-V*M*vertex);
    float rv=clamp(dot(r,v),0,1);
    
    rv=pow(rv,1);
    gl_Position=P*V*M*vertex;
 
 i_c = color*vec4(0,0,0,1) + color*vec4(1,1,1,1) *dot(n,l)+color*color*rv;
}
