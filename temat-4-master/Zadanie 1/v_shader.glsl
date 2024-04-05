#version 330

//Zmienne jednorodne
uniform mat4 P;
uniform mat4 V;
uniform mat4 M;

//Atrybuty
in vec4 vertex; //wspolrzedne wierzcholka w przestrzeni modelu
in vec4 normal; //wektor normalny w przestrzeni modelu
in vec4 color; //kolor skojarzony z wierzcho³kiem
in vec2 texCoord; //wspó³rzêdna teksturowana

out vec4 i_c;

void main(void) {
    i_c=color;

    vec4 polZroSwi=vec4(0,0,-6,1);
    vec4 l = normalize(V*polZroSwi-V*M*vertex);
    vec4 n = normalize(V*M*normal);
    i_c = color*vec4(0,0,0,1) + color*vec4(1,1,1,1) *dot(n,l);
    gl_Position=P*V*M*vertex;
}
