extends TextureButton

func _ready() -> void:
	disabled = true
	$AnimationPlayer.play("start_button_init")
	$AnimationPlayer2.play("button_animation")

func _on_pressed() -> void:
	$AnimationPlayer.play("start_button_pressed")

func active():
	disabled = false
