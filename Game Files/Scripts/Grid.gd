extends Node3D

const MAP_SIZE = 10
const TILE_SIZE = 1.8

signal grid_clicked(board_pos)
signal update_state_and_player(state, cur_player)
signal update_captured_tile(tile)

enum states {ADD, REMOVE}

var captured_tile
var captured_pieces:Array
var board = []

func _ready():
  generate_map()

  for i in MAP_SIZE:
          board.append([])
          for j in MAP_SIZE:
              board[i].append(3)
              
  make_starting_board_state()

  for child in self.get_children():
    child.tile_clicked.connect(on_tile_clicked)

func on_tile_clicked(board_pos):
  grid_clicked.emit(board_pos)

func generate_map():
  #Clear the map
  for child in self.get_children():
    self.remove_child(child)
    
  for x in range(-MAP_SIZE, MAP_SIZE):
    for y in range(-MAP_SIZE, MAP_SIZE):
      if in_map(x,y):
        add_tile(x,y)
  
func add_tile(x,y):
   
  var tile = preload("res://Scenes/hexagon.tscn").instantiate()
  var offset = 0.0 if !(x % 2) else TILE_SIZE / 2
  var margin = 0.25
  add_child(tile)
  tile.translate(Vector3(x * (TILE_SIZE - margin), 0 ,y * TILE_SIZE + offset))
  tile.cubic_pos = oddq_to_cube(Vector2(x,y))
  tile.board_pos = get_board_pos(get_label(Vector2(x,y)))
  tile.display_coords(get_label(Vector2(x,y)))
  
func make_starting_board_state():
  for child in self.get_children():
    child.state = 0
  for i in range(10): 
    for j in range(10): 
      if abs(i-j) < 6:
        board[i][j] = 0
        
func oddq_to_cube(hex):
  var q = hex.x
  var r = hex.y - (hex.x - (int(q)&1)) / 2
  return Vector3i(q, r, -q-r)
  
func in_map(x,y):
  var r = ceil(MAP_SIZE / 2)
  var cube = oddq_to_cube(Vector2(x,y))
  if cube.x <= r and cube.x >= -r and cube.y <= r and cube.y > -r and cube.z < r and cube.z >= -r:
    return true
  else:
    return false

func in_map_too(board_pos):
  if (board_pos.x >= 0 and board_pos.x <= 9 and board_pos.y >= 0 and board_pos.y <= 9 
  and abs(board_pos.x - board_pos.y) < 6):
    return true
  else:
    return false

func get_label(hex):
  var cube = oddq_to_cube(hex)
  var labels = ""
  labels += String.chr(65 + (6 - int(cube.y)) - 1)
  labels += str(6 + cube.z) 
  return labels
    
func get_board_pos(tile_id):
  var x = int(tile_id.substr(0,1).unicode_at(0) - "A".unicode_at(0))
  var y = tile_id.to_int() - 1  
  return Vector2(x,y)

func update_board_state(board_pos, value):
  board[board_pos.x][board_pos.y] = value
  for child in self.get_children():
    if child.board_pos == board_pos:
      child.state = value
  #print(board)

func play_move(board_pos, player, state):
  if state == states.ADD and board[board_pos.x][board_pos.y] == 0:
    update_board_state(board_pos, player)
    
    if !check_for_capture(board_pos).is_empty():
      captured_pieces = check_for_capture(board_pos)
      update_state_and_player.emit(states.REMOVE, player) #changes state in main
      return
      
    update_state_and_player.emit(states.ADD, 3-player)
    
    if captured_tile != null:
      update_board_state(captured_tile, 0)
      captured_tile = null
      update_captured_tile.emit(captured_tile)
      
  if state == states.REMOVE and captured_pieces.has(board_pos):
    if captured_tile != null:
      update_board_state(captured_tile, 0)
      
    captured_tile = board_pos
    update_captured_tile.emit(captured_tile)
    
    update_board_state(board_pos, 3)
    update_state_and_player.emit(states.ADD, 3-player)
    captured_pieces.clear()

func check_for_win():
  #TODO : Optimise this
  for i in range(10):
    var consecutive_count = 1
    for j in range(1, 10):
      if board[i][j] == board[i][j-1] and board[i][j] != 0:
        consecutive_count += 1
        if consecutive_count >= 5:
          return true
      else:
        consecutive_count = 1

  for j in range(10):
    var consecutive_count = 1
    for i in range(1,10):
      if board[i][j] == board[i-1][j] and board[i][j] != 0:
        consecutive_count += 1
        if consecutive_count >= 5:
          return true
      else:
        consecutive_count = 1
  
  #Upper triangle      
  for j in range(10-4):
    var consecutive_count = 1
    for i in range(1, 10-j):
      #print(i,",", j+i, " -> ", i-1, ",", i+j-1)
      if board[i][j+i] == board[i-1][j+i-1] and board[i][j+i] != 0:
        consecutive_count += 1
        if consecutive_count >= 5:
          return true
      else:
        consecutive_count = 1
    
  #Down triangle  
  for i in range(1, 10-4):
    var consecutive_count = 1
    for j in range(1, 10-i):
      #print(i+j,",", j, " -> ", i+j-1, ",", j-1)
      if board[i+j][j] == board[i+j-1][j-1] and board[i+j][j] != 0:
        consecutive_count += 1
        if consecutive_count >= 5:
          return true
      else:
        consecutive_count = 1
        
  return false

func check_for_capture(board_pos):

  var potential_removes : Array
  var directions : Array
  directions.resize(6)
  directions[0] = Vector2i(1,1)
  directions[1] = Vector2i(-1,-1)
  directions[2] = Vector2i(1,0)
  directions[3] = Vector2i(-1,0)
  directions[4] = Vector2i(0,1)
  directions[5] = Vector2i(0,-1)
  var x = board_pos.x
  var y = board_pos.y
  for i in range(6):
    if in_map_too(board_pos + 3 * directions[i]):
      var dx = directions[i].x
      var dy = directions[i].y
      
      if ((board[x][y] == 1 and board[x + 1 * dx][y + 1 * dy] == 2 
      and  board[x + 2 * dx][y + 2 * dy] == 2 and  board[x + 3 * dx][y + 3 * dy] == 1)
      or ( board[x][y] == 2 and  board[x + 1 * dx][y + 1 * dy] == 1 
      and  board[x + 2 * dx][y + 2 * dy] == 1 and  board[x + 3 * dx][y + 3 * dy] == 2)):
        potential_removes.append(Vector2i(x + 1 * dx, y + 1 * dy))
        potential_removes.append(Vector2i(x + 2 * dx, y + 2 * dy))
      
  return potential_removes
