extends TextureButton

func _ready() -> void:
	disabled = true
	$SettingButton.disabled = true
	$AnimationPlayer.play("start_button_init")

func _on_pressed() -> void:
	$AnimationPlayer.play("start_button_pressed")

func active():
	disabled = false
	$SettingButton.disabled = false
