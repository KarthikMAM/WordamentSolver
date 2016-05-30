# WordamentSolverAlgorithm
An algorithm to solve Wordament solver by planning and conflict resolution


* I have also designed a GUI for this algorithm in C# WPF Framework. The link is <a href="https://github.com/KarthikMAM/WordamentSolver">WordamentSolver</a>.
* This algorithm is an improvement over the previous one which I have designed.

Improvements
* Iterative algorithm
* Uses sets for conflict resolution
* Creates a global state with the past and the present events
* Explanation and Computations done in a single go unlike two sweeps in my previous algorithm
* This is very scalable
  * that is it can include compund word search in the boxes
  * it can be used over a dynamic range of matrix sizes.
