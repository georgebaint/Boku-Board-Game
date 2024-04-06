extends Node3D

@onready var grid = $Grid
@onready var victory_popup = $Victory_popup
@onready var panel = $StartPanel
@onready var move_log = $Move_log

var AI_script = load("res://CSharpFiles/AI_script.cs")

enum states {ADD, REMOVE, GAME_OVER} 
var state = states.ADD

enum modes {PvAI_1, PvAI_2, PvP}
var mode = modes.PvAI_1

var player1_type : String
var player2_type : String
var cur_player = 1 
var AI = AI_script.new()
var captured_tile : Vector2i


func _ready():
  victory_popup.visible = false
  grid.grid_clicked.connect(on_grid_clicked)
  grid.update_state_and_player.connect(on_grid_updated)
  grid.update_captured_tile.connect(on_tile_captured)
  captured_tile = Vector2i(-1, -1)
  grid.visible = false

func _process(delta):

  if get_player_type() == "AI":
    if state != states.GAME_OVER:
      
      var AI_move = AI.Play(grid.board, cur_player, captured_tile) # return tuple of ADD,REMOVE moves of AI. If !REMOVE return null.
      print(AI_move)
      var add_move = Vector2i(AI_move[0], AI_move[1])
      var remove_move = Vector2i(AI_move[2], AI_move[3])     
      move_log.print_move(add_move, cur_player, state)  
      grid.play_move(add_move, cur_player, state) #ADD
      
      if remove_move[0] > -1 and remove_move[1] > -1: #REMOVE
        move_log.print_move(remove_move, cur_player, state)
        grid.play_move(remove_move, cur_player, state)

      win_checking()

func on_grid_clicked(board_pos):
  if get_player_type() == "Human":
    if state != states.GAME_OVER :
      move_log.print_move(board_pos, cur_player, state)      
      grid.play_move(board_pos, cur_player, state)
      win_checking()

func _on_option_button_item_selected(index):
  if index == 0:
    mode = modes.PvAI_1
  elif index == 1:
    mode = modes.PvAI_2
  elif index == 2:
    mode = modes.PvP

func game_start():
  if mode == modes.PvAI_1:
    player1_type = "Human"
    player2_type = "AI"
  elif mode == modes.PvAI_2:
    player1_type = "AI"
    player2_type = "Human"
  elif mode == modes.PvP:
    player1_type = "Human" 
    player2_type = "Human"
   
func _on_start_button_pressed():
  grid.visible = true
  panel.visible = false
  game_start()
  
func get_player_type():
  if cur_player == 1:
    return player1_type
  return player2_type

func on_grid_updated(_state, _cur_player):
  state = _state
  cur_player = _cur_player

func win_checking():
  if grid.check_for_win():
    victory_popup.visible = true
    # White and Black are switched because it means who is about to play
    if cur_player == 1:
      victory_popup.text = "Black wins!"
    elif cur_player == 2:
      victory_popup.text = "White wins"

    state = states.GAME_OVER

func on_tile_captured(tile):
  if tile != null:
    captured_tile = tile
  else: 
    captured_tile = Vector2i(-1,-1)
  print("Captured tile", captured_tile)

func _on_reset_button_pressed():
  reset_game()

func reset_game():
  state = states.ADD
  grid.make_starting_board_state()
  victory_popup.visible = false
  move_log.reset()
  cur_player = 1
