% Place squares in a line while preserving correct connections
% contains only limited number of square types
par int: width = 20;
par int: height = 20;

enum Square = {UpRight, Closed, Open};
enum Connection = {Up, Down, Both, No};

array[1..width, 1..height] of var Square: squares;
array[1..width+1, 1..height] of var Connection: horizConnections;
array[1..width, 1..height+1] of var Connection: vertConnections;

% Horizontal connection constraints
constraint forall(i in 1..width, j in 1..height)(connectionsHoriz(i, j, UpRight, No, Both));
constraint forall(i in 1..width, j in 1..height)(connectionsHoriz(i, j, Closed, No, No));
constraint forall(i in 1..width, j in 1..height)(connectionsHoriz(i, j, Open, Both, Both));

% Vertical connection constraints
constraint forall(i in 1..width, j in 1..height)(connectionsVert(i, j, UpRight, No, Both));
constraint forall(i in 1..width, j in 1..height)(connectionsVert(i, j, Closed, No, No));
constraint forall(i in 1..width, j in 1..height)(connectionsVert(i, j, Open, Both, Both));

predicate connectionsHoriz(int: i, int:j, Square: square, var Connection: left, var Connection: right) =
  (squares[i, j] = square) -> (horizConnections[i, j] = left /\ horizConnections[i + 1, j] = right);

predicate connectionsVert(int: i, int:j, Square: square, var Connection: bottom, var Connection: top) =
  (squares[i, j] = square) -> (vertConnections[i, j] = bottom /\ vertConnections[i, j + 1] = top);
  
% Number of squares constraint
constraint (sum([1 | i in 1..width, j in 1..height where squares[i, j] = Closed]) > 1);
constraint (sum([1 | i in 1..width, j in 1..height where squares[i, j] = Open]) > 1);

solve satisfy;

array[Square] of string: squareView = ["|_", "||", "__"];
output [squareView[fix(squares[i, j])] ++ if i==width then "\n" else "" endif | j in 1..height, i in 1..width];