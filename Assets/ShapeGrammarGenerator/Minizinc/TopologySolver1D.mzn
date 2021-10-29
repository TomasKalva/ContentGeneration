% Place squares in a line while preserving correct connections
par int: n = 5;

enum Square = {LeftClosed, Closed, Open};
enum Connection = {Left, Right, Both, No};

array[1..n] of var Square: squares;
array[1..n+1] of var Connection: connections;

% Connection constraints
constraint forall(i in 1..n)(
  (squares[i] = LeftClosed) -> (connections[i] = No /\ connections[i+1] = Both));
constraint forall(i in 1..n)(
  (squares[i] = Closed) -> (connections[i] = No /\ connections[i+1] = No));
constraint forall(i in 1..n)(
  (squares[i] = Open) -> (connections[i] = Both /\ connections[i+1] = Both));

% Number of squares constraint
constraint sum([1 | i in 1..n where squares[i] = Closed]) > 1;
constraint sum([1 | i in 1..n where squares[i] = Open]) > 1;

solve satisfy;

array[Square] of string: squareView = ["|_", "||", "__"];
output [squareView[fix(squares[i])] | i in 1..n];