using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerNew : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;
    private PlayerMotor motor;

    public ThirdPersonCamera cameraController;

    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;
        motor = GetComponent<PlayerMotor>();

        onFoot.Jump.performed += ctx => motor.Jump();
        onFoot.Sprint.performed += ctx => motor.Sprint();

        onFoot.Enable();
    }

    void Update()
    {
        if (motor.inputFrozen)
            return;
        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }

    void LateUpdate()
    {
        if (motor.inputFrozen)
            return;
        cameraController.ProcessLook(onFoot.Look.ReadValue<Vector2>());
    }

    void OnDisable()
    {
        onFoot.Disable();
    }
}