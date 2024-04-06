extends Node3D

@onready var black_sphere = $Black_sphere
@onready var white_sphere = $White_sphere
@onready var click_detection_area = $Click_detection_area
@onready var label_3d = $Label3D
@onready var remove_indicator = $Remove_indicator
@onready var blocked_indicator = $Blocked_indicator

signal tile_clicked(_board_pos)

var board_pos : Vector2i
var cubic_pos : Vector3i
var state : int :
  set(v):
    state = v
    if v == 0:
      white_sphere.visible = false
      black_sphere.visible = false
      blocked_indicator.visible = false
    elif v == 1:
      white_sphere.visible = true
      black_sphere.visible = false
      blocked_indicator.visible = false
    elif v == 2:
      white_sphere.visible = false
      black_sphere.visible = true
      blocked_indicator.visible = false
    elif v == 3:
      white_sphere.visible = false
      black_sphere.visible = false
      blocked_indicator.visible = true

func _ready():
  state = 0
  remove_indicator.visible = false 

func display_coords(text):
  label_3d.text = text

func _on_click_detection_area_input_event(camera, event, position, normal, shape_idx):
  if event is InputEventMouseButton:
    if event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
        tile_clicked.emit(board_pos)
