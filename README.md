# Disjointed
![]()
Disjointed este un joc 3D Puzzle/Platformer într-o lume de jucării abandonate. Protagonista este o păpușă cu sforile rupte pe care le folosește pentru a se deplasa și controla celelalte jucării din jurul ei.

## Game Mechanics
* **Sprinting.**
* **Variable Jump.** Poți sări mai sus după timpul in care butonul este apăsat.
* **Swinging.** Te poți agăța și arunca de pe anumite obiecte pentru a te arunca in locuri altfel inaccesibile.
* **Controlling Enemies.** Poți controla celelalte jucării.
* **Grabbing and Throwing Objects.** Poți trage si arunca obiecte.

## **Detalii Tehnice**
Jocul este creat in Unity 2022.3 LTS, fiind programat in C#. Grafica este realizată printr-un Render Pipeline custom numit Eclipse iar mecanicile jocului sunt realizate printr-un framework custom numit Enigma.

### Eclipse Render Pipeline 
Eclipse este un Render Pipeline creat special pentru Disjointed care înlocuiește partea de rendering din Unity (in loc de URP/HDRP/Built-In). Programat în o combinație de HLSL si C#

**Features:**
* **Physically Based Rendering.** Materialele au suport pentru Specularity, Roughness și Metalness controlate prin texturi.
* **Texture Inversion/Tint**. Texturile pot fi colorate sau inversate direct din material (ex: Smoothness Texture -> Roughness fara a necesita alt shader).
* **Sheen.** Suport pentru luciu (ex: materiale textile) similar cu implementarea din Blender Eevee.
* **Clearcoat.** Suport pentru un al doilea strat de reflecție 
* **3 Tipuri de umbre.** 
      Hard Shadows (unfiltered)
      Soft Shadows (Gaussian Blur)
      Distance Soft Shadows (Distance Based Gaussian Blur)
* **Cascaded Shadow Mapping**. Suport pentru până la 4 pentru lumini direcționale.
* **Cubemap Based Lighting**. Suport pentru Cubemap Reflections dar si approximated Indirect Lighting prin Reflection Probes.
* **Shader modular**. Toate partile din rendering sunt calculate în fișiere HLSL separate pentru a crea cat mai ușor variante pentru diferite necesități.

### Enigma Framework
Enigma este un framework creat pentru a face mai ușor procesul de a crea caractere cu diverse abilități prin entități și acțiuni.

Mecanicile jocului sunt implementate folosind acțiuni care schimba starea entității controlate. Jucatorul controleaza acțiunile entității prin ActionInput.

**ActionInput** este un sistem care apeleaza la InputManager-ul din Unity pentru a-i extinde functionalitatea (Rebindable Inputs, Converting Analog to Binary Inputs)

### Shape Trail Renderer
**Shape Trail** este o componenta custom pentru a crea trailuri de o lungime fixa, cu gravitație si collision. Fizica este calculata in C# printr-o lista de puncte iar Modelul final este generat printr-un Compute Shader ce creeaza un tub dintr-un base mesh in fiecare punct 

------

 
## Inspirații 
* Alice Madness Returns
American McGee's Alice
* Deltarune
* Little Inferno
* Half-Life 2
