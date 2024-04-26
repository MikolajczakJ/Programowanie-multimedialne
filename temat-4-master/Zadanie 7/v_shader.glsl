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
in vec2 tex1Coord; //wsp�rz�dna teksturowana

out vec4 i_c;
out vec4 i_v;
out vec4 i_n;
out vec2 i_tc;
out vec2 i_tc2;

void main(void) {
    i_c=color;
    i_v = vertex;
    i_n = normal;
    i_tc = texCoord;
    i_tc2 = tex1Coord;
    gl_Position=P*V*M*vertex;

}