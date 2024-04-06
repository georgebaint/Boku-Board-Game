using System;
using System.Collections.Generic;
using Godot;

public partial class AI_script : RefCounted // Does everything needed for the AI
{
  public static bool InMap(int[] pos)
  {
    /*
    Function that checks if matrix coordinates are inside the game map (C# Edition)
    */
    if (pos[0] >= 0 && pos[0] <= 9 && pos[1] >= 0 && pos[1] <= 9 && Math.Abs(pos[0] - pos[1]) < 6)
    {
      return true;
    }
    return false;
  }

  public Vector4I Play(Godot.Collections.Array _board, int player, Vector2I capturedTile)
  {
    /* 
    Function that GDScript calls when AI is to Play
    */

    // Convert Board type from GDScript array[array] to C# int[,]
    int[,] board = new int[10, 10];
    for (int i = 0; i < 10; i++)
    {
      for (int j = 0; j < 10; j++)
      {
        board[i, j] = _board[i].As<Godot.Collections.Array>()[j].As<int>();
      }
    }

    // Depth of search
    int depth = 3;

    // Convert Previous captured tile type from Vector2I to C# int[] 
    int[] prevCapturedTile = new int[] { capturedTile.X, capturedTile.Y };

    // Call AlphaBeta MiniMax function
    var tuple = MiniMax(board, depth, -int.MaxValue, int.MaxValue, player, prevCapturedTile);
    int bestScore = tuple.Item1;
    int[] bestMove = tuple.Item2;
    GD.Print("AlphaBeta Done");
    GD.Print("Best score: ", bestScore);
    GD.Print("Best move: ", bestMove[0], ",", bestMove[1]);

    // Convert best move from C# int[] to GDScript Vector4I
    Vector4I pos = new(bestMove[0], bestMove[1], bestMove[2], bestMove[3]);

    return pos;
  }

  public static bool IsTerminalNode(int[,] board)
  {
    /* 
    Function that checks if we have a terminal state
    */

    // Normally I should have checked if a win or a loss occurs but I do this indirectly by giving an extreme evaluation score for these states.
    // This means that my tree continues to search even after a terminal win/lose node which is suboptimal but this is what works best for my implementation.

    // Check for WIN or LOSS
    if ((EvaluateBoard(board) > 500000) || (EvaluateBoard(board) < -500000))
    {
      return true;
    }
    // Check for DRAW (no 0s are left)
    for (int i = 0; i < 10; i++)
    {
      for (int j = 0; j < 10; j++)
      {
        if (board[i, j] == 0)
        {
          return false;
        }
      }
    }

    return true;
  }

  public static int EvaluateBoard(int[,] board)
  {
    /*
    Function that evaluates the current board state
    */

    int w1 = 1; // Weight for placing near the center
    int w2 = 5;   // Weight for more hexes occupied
    int w3 = 5;  // Weight for more "3s in a row"
    int w4 = 10;  // Weight for more "4s in a row"
    int w5 = 1000000; // Weight for win

    int feature1 = CalculateDistanceFromCenter(board); // Distance from Center of Map
    int feature2 = CalculateHexCountDifference(board); // Difference between the number of player pieces
    int feature3 = CalculateKInARowDifference(board, 3); // Difference between the 3s in a row each player has
    int feature4 = CalculateKInARowDifference(board, 4); // Difference between the 4s in a row each player has
    int feature5 = CalculateKInARowDifference(board, 5); // Difference between the 5s in a row each player has

    int totalScore = w1 * feature1 + w2 * feature2 + w3 * feature3 + w4 * feature4 + w5 * feature5;

    return totalScore;

  }

  public static int CalculateDistanceFromCenter(int[,] board)
  {
    int boardSize = board.GetLength(0);

    // Define the center coordinates of the board.
    int centerX = boardSize / 2;
    int centerY = boardSize / 2;

    int totalScore = 0;
    const int maxDistance = 7; // int((5^2 + 5^2)^(1/2))
    // Loop through the board to calculate the score.
    for (int i = 0; i < boardSize; i++)
    {
      for (int j = 0; j < boardSize; j++)
      {

        // Calculate the distance from the center.
        int distance = (int)Math.Sqrt(Math.Pow(i - centerX, 2) + Math.Pow(j - centerY, 2));
        if (board[i, j] == 1)
        {
          // Assign a higher score for marbles closer to the center.
          totalScore += (maxDistance - distance);
        }
        else if (board[i, j] == 2) // Assuming 2 represents the opponent's marbles.
        {

          // Subtract a higher score for opponent's marbles closer to the center.
          totalScore -= (maxDistance - distance);
        }
      }
    }
    return totalScore;
  }

  public static int CalculateKInARowDifference(int[,] board, int k)
  {
    int firstPlayer = 1;
    int secondPlayer = 2;

    int firstPlayerKInARow = 0;
    int secondPlayerKInARow = 0;

    int boardSize = board.GetLength(0);

    // Horizontal
    for (int i = 0; i < boardSize; i++)
    {
      int consecutiveCount = 1;
      for (int j = 1; j < boardSize; j++)
      {
        if (board[i, j] == board[i, j - 1] && board[i, j] != 0 && board[i, j - 1] != 3)
        {
          consecutiveCount++;
        }
        else
        {
          if (consecutiveCount == k)
          {
            if (board[i, j - 1] == firstPlayer)
            {
              firstPlayerKInARow++;
            }
            else if (board[i, j - 1] == secondPlayer)
            {
              secondPlayerKInARow++;
            }

          }
          consecutiveCount = 1;
        }
      }
      if (consecutiveCount == k)
      {
        if (board[i, boardSize - 1] == firstPlayer)
        {
          firstPlayerKInARow++;
        }
        else if (board[i, boardSize - 1] == secondPlayer)
        {
          secondPlayerKInARow++;
        }

      }
    }

    // Vertical
    for (int j = 0; j < boardSize; j++)
    {
      int consecutiveCount = 1;
      for (int i = 1; i < boardSize; i++)
      {
        if (board[i, j] == board[i - 1, j] && board[i, j] != 0 && board[i, j] != 3)
        {
          consecutiveCount++;
        }
        else
        {
          if (consecutiveCount == k)
          {
            if (board[i - 1, j] == firstPlayer)
            {
              firstPlayerKInARow++;
            }
            else if (board[i - 1, j] == secondPlayer)
            {
              secondPlayerKInARow++;
            }
          }
          consecutiveCount = 1;
        }
      }
      if (consecutiveCount == k)
      {
        if (board[boardSize - 1, j] == firstPlayer)
        {
          firstPlayerKInARow++;
        }
        else if (board[boardSize - 1, j] == secondPlayer)
        {
          secondPlayerKInARow++;
        }
      }
    }

    // Upper triangle
    for (int j = 0; j < boardSize - 4; j++)
    {
      int consecutiveCount = 1;
      for (int i = 1; i < boardSize - j; i++)
      {
        if (board[i, j + i] == board[i - 1, j + i - 1] && board[i, j + i] != 0 && board[i, j + i] != 3)
        {
          consecutiveCount++;
        }
        else
        {
          if (consecutiveCount == k)
          {
            if (board[i - 1, j + i - 1] == firstPlayer)
            {
              firstPlayerKInARow++;
            }
            else if (board[i - 1, j + i - 1] == secondPlayer)
            {
              secondPlayerKInARow++;
            }
          }
          consecutiveCount = 1;
        }
      }
      if (consecutiveCount == k)
      {
        if (board[boardSize - j - 1, boardSize - 1] == firstPlayer)
        {
          firstPlayerKInARow++;
        }
        else if (board[boardSize - j - 1, boardSize - 1] == secondPlayer)
        {
          secondPlayerKInARow++;
        }
      }
    }

    // Down triangle
    for (int i = 1; i < boardSize - 4; i++)
    {
      int consecutiveCount = 1;
      for (int j = 1; j < boardSize - i; j++)
      {
        if (board[i + j, j] == board[i + j - 1, j - 1] && board[i + j, j] != 0 && board[i + j, j] != 3)
        {
          consecutiveCount++;
        }
        else
        {
          if (consecutiveCount == k)
          {
            if (board[i + j - 1, j - 1] == firstPlayer)
            {
              firstPlayerKInARow++;
            }
            else if (board[i + j - 1, j - 1] == secondPlayer)
            {
              secondPlayerKInARow++;
            }
          }
          consecutiveCount = 1;
        }
      }
      if (consecutiveCount == k)
      {
        if (board[boardSize - 1, boardSize - i - 1] == firstPlayer)
        {
          firstPlayerKInARow++;
        }
        else if (board[boardSize - 1, boardSize - i - 1] == secondPlayer)
        {
          secondPlayerKInARow++;
        }
      }
    }

    return firstPlayerKInARow - secondPlayerKInARow;
  }

  public static int CalculateHexCountDifference(int[,] board)
  {
    int firstPlayer = 1;
    int secondPlayer = 2;

    int firstPlayerCount = 0;
    int secondPlayerCount = 0;

    int boardSize = board.GetLength(0);

    for (int i = 0; i < boardSize; i++)
    {
      for (int j = 0; j < boardSize; j++)
      {
        if (board[i, j] == firstPlayer)
        {
          firstPlayerCount++;
        }
        else if (board[i, j] == secondPlayer)
        {
          secondPlayerCount++;
        }
      }
    }

    return firstPlayerCount - secondPlayerCount;
  }

  public List<int[]> CheckForCapture(int[,] board, int[] Pos, int player)
  {
    /*
    Function that returns a list of all pieces that can be captured
    If there are no potential remove options return an empty list
    */

    List<int[]> potentialRemoves = new List<int[]>();
    List<int[]> directions = new List<int[]>
    {
        new int[2] { 1, 1 },
        new int[2] { -1, -1 },
        new int[2] { 1, 0 },
        new int[2] { -1, 0 },
        new int[2] { 0, 1 },
        new int[2] { 0, -1 }
    };
    int x = Pos[0];
    int y = Pos[1];
    int otherPlayer = 3 - player;

    // For each direction
    for (int i = 0; i < 6; i++)
    {
      int[] temp = new int[2]; // Temp value that helps to check if position + 3*direction is in Map
      for (int k = 0; k < 2; k++)
      {
        temp[k] = 3 * directions[i][k];
        temp[k] += Pos[k];
      }
      if (InMap(temp)) // Needed so not check outside of map
      {
        int dx = directions[i][0];
        int dy = directions[i][1];

        // Check if we have a capture sequence for player
        if (board[x + 1 * dx, y + 1 * dy] == otherPlayer
        && board[x + 2 * dx, y + 2 * dy] == otherPlayer
        && board[x + 3 * dx, y + 3 * dy] == player)
        {
          // Append the pieces that can be captured in the list
          potentialRemoves.Add(new int[] { x + 1 * dx, y + 1 * dy });
          potentialRemoves.Add(new int[] { x + 2 * dx, y + 2 * dy });
        }
      }
    }
    return potentialRemoves;
  }

  public List<int[]> FindSuccessors(int[,] board, int player)
  {
    /*
    Function that returns a list with all the possible moves for a specific board state
    Move is comprised of 4 components: 2 for Adding a new piece and 2 for removing a piece
    If an addition occurs without removing, removal components are equal to -1
    */

    List<int[]> childlist = new List<int[]>();
    for (int i = 0; i < board.GetLength(0); i++)
    {
      for (int j = 0; j < board.GetLength(1); j++)
      {
        if (board[i, j] == 0)
        {
          int[] curPos = new int[2];
          curPos[0] = i;
          curPos[1] = j;
          List<int[]> removeOptions = CheckForCapture(board, curPos, player);

          if (removeOptions.Count != 0)
          {
            for (int k = 0; k < removeOptions.Count; k++)
            {
              int[] moveInfo = new int[] { i, j, removeOptions[k][0], removeOptions[k][1] };
              childlist.Add(moveInfo);
            }
          }
          else
          {
            int[] moveInfo = new int[] { i, j, -1, -1 };
            childlist.Add(moveInfo);
          }
        }
      }
    }
    return childlist;
  }

  public static (int[,], int[]) DoMove(int[,] board, int[] moveInfo, int player, int[] prevCaptured)
  {
    /*
    Function that plays a move by player in the board
    Returns the updated board and the position of the captured hex
    If there is no captured hex return (-1,-1) instead
    */

    int[,] newBoard = (int[,])board.Clone();
    // Add the new move to the board
    newBoard[moveInfo[0], moveInfo[1]] = player;

    int[] capturedHex = new int[2] { -1, -1 };

    // If a previous captured hex existed then clear it
    if (prevCaptured[0] != -1)
    {
      newBoard[prevCaptured[0], prevCaptured[1]] = 0;
    }

    // If the move has a Removal component too, capture that piece
    if (moveInfo[2] != -1 && moveInfo[3] != -1)
    {
      newBoard[moveInfo[2], moveInfo[3]] = 3;
      capturedHex = new int[2] { moveInfo[2], moveInfo[3] };
    }

    return (newBoard, capturedHex);
  }

  public (int, int[]) MiniMax(int[,] board, int depth, int alpha, int beta, int player, int[] prevCapture)
  {
    /*
    A standard ab MiniMax function modified to keep the previous captured hex from the previous depth
    Returns best Score and Best Move
    */

    // Check if the current node is terminal or the depth limit is reached
    if (IsTerminalNode(board) || depth == 0)
    {
      int staticScore = EvaluateBoard(board); // Evaluate the current board
      return (staticScore, new int[4]); // Return the static score and an empty move
    }

    // Find the available moves for the current player.
    List<int[]> childMoveInfo = FindSuccessors(board, player);
    int numSuccessors = childMoveInfo.Count;

    if (player == 1) // Maximizing player's turn.
    {
      int[] move = new int[4];
      int maxEval = -int.MaxValue; // Initialize maxEval to negative infinity

      for (int child = 0; child < numSuccessors; child++)
      {
        var (newBoard, capturedHex) = DoMove(board, childMoveInfo[child], player, prevCapture); // Apply a move

        var tuple = MiniMax(newBoard, depth - 1, alpha, beta, 2, capturedHex); // Recursively call MiniMax
        int score = tuple.Item1;

        if (score > maxEval)
        {
          maxEval = score; // Update maxEval if a better move is found
          move = childMoveInfo[child]; // Update the best move
        }
        alpha = Math.Max(alpha, maxEval); // Update alpha

        if (beta <= alpha)
        {
          break; // Prune the search if a better move is not possible
        }
      }
      return (maxEval, move); // Return the best score and move
    }
    else // Minimizing player's turn
    {
      int[] move = new int[4];
      int minEval = int.MaxValue; // Initialize minEval to positive infinity

      for (int child = 0; child < numSuccessors; child++)
      {
        var (newBoard, capturedHex) = DoMove(board, childMoveInfo[child], player, prevCapture); // Apply a move

        var tuple = MiniMax(newBoard, depth - 1, alpha, beta, 1, capturedHex); // Recursively call MiniMax
        int score = tuple.Item1;

        if (score < minEval)
        {
          minEval = score; // Update minEval if a better move is found
          move = childMoveInfo[child]; // Update the best move
        }
        beta = Math.Min(beta, minEval); // Update beta

        if (beta <= alpha)
        {
          break; // Prune the search if a better move is not possible
        }
      }
      return (minEval, move); // Return the best score and move
    }
  }

}

// public class TranspositionTableEntry
// {
//   public int Depth { get; set; }
//   public int Score { get; set; }
//   public TranspositionTableFlag Flag { get; set; }
// }

// public enum TranspositionTableFlag
// {
//   Exact,
//   LowerBound,
//   UpperBound
// }

// public class TranspositionTable
// {
//   private Dictionary<int, TranspositionTableEntry> table = new Dictionary<int, TranspositionTableEntry>();

//   public bool TryGet(int hash, out TranspositionTableEntry entry)
//   {
//     return table.TryGetValue(hash, out entry);
//   }

//   public void Store(int hash, TranspositionTableEntry entry)
//   {
//     table[hash] = entry;
//   }
// }

// public (int, int[]) MiniMax(int[,] board, int depth, int alpha, int beta, int player, int[] prevCapture, TranspositionTable transpositionTable)
// {
//   if (IsTerminalNode(board) || depth == 0)
//   {
//     int staticScore = EvaluateBoard(board);
//     return (staticScore, new int[4]);
//   }

//   int hash = ComputeHash(board); // Calculate a unique hash for the board state.

//   if (transpositionTable.TryGet(hash, out TranspositionTableEntry entry) && entry.Depth >= depth)
//   {
//     if (entry.Flag == TranspositionTableFlag.Exact)
//     {
//       return (entry.Score, new int[4]);
//     }
//     else if (entry.Flag == TranspositionTableFlag.LowerBound)
//     {
//       alpha = Math.Max(alpha, entry.Score);
//     }
//     else if (entry.Flag == TranspositionTableFlag.UpperBound)
//     {
//       beta = Math.Min(beta, entry.Score);
//     }
//     if (alpha >= beta)
//     {
//       return (entry.Score, new int[4]);
//     }
//   }

//   List<int[]> childMoveInfo = FindSuccessors(board, player);
//   int numSuccessors = childMoveInfo.Count;

//   int[] move = new int[4];
//   int bestScore = (player == 1) ? int.MinValue : int.MaxValue;

//   for (int child = 0; child < numSuccessors; child++)
//   {
//     var (newBoard, capturedHex) = DoMove(board, childMoveInfo[child], player, prevCapture);
//     prevCapture = capturedHex;

//     var tuple = MiniMax(newBoard, depth - 1, alpha, beta, 3 - player, prevCapture, transpositionTable);
//     int value = tuple.Item1;

//     if (player == 1)
//     {
//       if (value > bestScore)
//       {
//         bestScore = value;
//         move = childMoveInfo[child];
//       }
//       alpha = Math.Max(alpha, bestScore);
//     }
//     else
//     {
//       if (value < bestScore)
//       {
//         bestScore = value;
//         move = childMoveInfo[child];
//       }
//       beta = Math.Min(beta, bestScore);
//     }

//     if (alpha >= beta)
//     {
//       break;
//     }
//   }

//   TranspositionTableFlag flag = TranspositionTableFlag.Exact;
//   if (bestScore <= alpha)
//   {
//     flag = TranspositionTableFlag.UpperBound;
//   }
//   else if (bestScore >= beta)
//   {
//     flag = TranspositionTableFlag.LowerBound;
//   }

//   var entry = new TranspositionTableEntry
//   {
//     Depth = depth,
//     Score = bestScore,
//     Flag = flag
//   };
//   transpositionTable.Store(hash, entry);

//   return (bestScore, move);
// }

// public class TranspositionTableEntry
// {
//   public int Depth { get; set; }
//   public int Score { get; set; }
//   public TranspositionTableFlag Flag { get; set; }
// }

// public enum TranspositionTableFlag
// {
//   Exact,
//   LowerBound,
//   UpperBound
// }

// public class TranspositionTable
// {
//   private Dictionary<int, TranspositionTableEntry> table = new Dictionary<int, TranspositionTableEntry>();

//   public bool TryGet(int hash, out TranspositionTableEntry entry)
//   {
//     return table.TryGetValue(hash, out entry);
//   }

//   public void Store(int hash, TranspositionTableEntry entry)
//   {
//     table[hash] = entry;
//   }
// }

// public (int, int[]) MiniMaxwithTT(int[,] board, int depth, int alpha, int beta, int player, int[] prevCapture, TranspositionTable transpositionTable)
// {
//   if (IsTerminalNode(board) || depth == 0)
//   {
//     int staticScore = EvaluateBoard(board);
//     return (staticScore, new int[4]);
//   }

//   int hash = ComputeHash(board); // Calculate a unique hash for the board state.

//   if (transpositionTable.TryGet(hash, out TranspositionTableEntry entry) && entry.Depth >= depth)
//   {
//     if (entry.Flag == TranspositionTableFlag.Exact)
//     {
//       return (entry.Score, new int[4]);
//     }
//     else if (entry.Flag == TranspositionTableFlag.LowerBound)
//     {
//       alpha = Math.Max(alpha, entry.Score);
//     }
//     else if (entry.Flag == TranspositionTableFlag.UpperBound)
//     {
//       beta = Math.Min(beta, entry.Score);
//     }
//     if (alpha >= beta)
//     {
//       return (entry.Score, new int[4]);
//     }
//   }

//   List<int[]> childMoveInfo = FindSuccessors(board, player);
//   int numSuccessors = childMoveInfo.Count;

//   int[] move = new int[4];
//   int bestScore = (player == 1) ? int.MinValue : int.MaxValue;

//   for (int child = 0; child < numSuccessors; child++)
//   {
//     var (newBoard, capturedHex) = DoMove(board, childMoveInfo[child], player, prevCapture);
//     prevCapture = capturedHex;

//     var tuple = MiniMax(newBoard, depth - 1, alpha, beta, 3 - player, prevCapture, transpositionTable);
//     int value = tuple.Item1;

//     if (player == 1)
//     {
//       if (value > bestScore)
//       {
//         bestScore = value;
//         move = childMoveInfo[child];
//       }
//       alpha = Math.Max(alpha, bestScore);
//     }
//     else
//     {
//       if (value < bestScore)
//       {
//         bestScore = value;
//         move = childMoveInfo[child];
//       }
//       beta = Math.Min(beta, bestScore);
//     }

//     if (alpha >= beta)
//     {
//       break;
//     }
//   }

//   TranspositionTableFlag flag = TranspositionTableFlag.Exact;
//   if (bestScore <= alpha)
//   {
//     flag = TranspositionTableFlag.UpperBound;
//   }
//   else if (bestScore >= beta)
//   {
//     flag = TranspositionTableFlag.LowerBound;
//   }

//   var entry = new TranspositionTableEntry
//   {
//     Depth = depth,
//     Score = bestScore,
//     Flag = flag
//   };
//   transpositionTable.Store(hash, entry);

//   return (bestScore, move);
// }
// public int ComputeHash(int[,] board)
// {
//     int hash = 17; // Start with an arbitrary prime number.

//     int boardSize = board.GetLength(0);

//     for (int i = 0; i < boardSize; i++)
//     {
//         for (int j = 0; j < boardSize; j++)
//         {
//             int piece = board[i, j];

//             // Incorporate the state of each cell into the hash.
//             hash = hash * 31 + piece.GetHashCode(); // You can choose a different prime number.

//             // Additionally, you may want to include the position itself in the hash.
//             hash = hash * 31 + i;
//             hash = hash * 31 + j;
//         }
//     }

//     return hash;
// }
