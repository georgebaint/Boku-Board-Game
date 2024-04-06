extends Control

@onready var text_edit = $PanelContainer/TextEdit
@onready var rich_text_label = $PanelContainer/RichTextLabel

var line_idx
enum states {ADD, REMOVE, GAME_OVER} 

func _ready():
  line_idx = 0
  
func print_move(board_pos, player, state):
  var move_text : String
  var label = board_pos_to_label(board_pos)
  
  if state == states.ADD:
    move_text = "Played " + label + " by Player " + str(player) + "."
  elif state == states.REMOVE:
    move_text = "Removed " + label + " by Player " + str(player) + "."
  
  #text_edit.insert_line_at(line_idx, move_text)
  rich_text_label.add_text(move_text)
  rich_text_label.newline()
  

  #print(move_text)
  line_idx += 1
  rich_text_label.scroll_to_line(rich_text_label.get_line_count()-1)

func board_pos_to_label(board_pos):
  var label = ""
  label += String.chr(board_pos.x + 65) 
  label += str(board_pos.y + 1)

  return label

func reset():
  rich_text_label.clear()
