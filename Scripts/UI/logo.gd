extends Control

@onready var animation_player = $AnimationPlayer
var skipped = false;

func _ready() -> void:
	animation_player.play("logo_animation")

func on_animation_finished():
	get_tree().change_scene_to_file("res://Scenes/Main.tscn")

func _input(event: InputEvent) -> void:
	if event.is_action_type():
		if animation_player.is_playing() and skipped == false and animation_player.current_animation_position < 4:
			animation_player.seek(4)
			skipped = true;
