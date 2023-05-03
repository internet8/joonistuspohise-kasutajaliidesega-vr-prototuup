using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Inputs : MonoBehaviour
{
    public InputDevice rightController;
    public bool drawing = false;
    public bool clearing = false;
    public bool inserting = false;
    public bool repeating = false;
    // Start is called before the first frame update
    void Start()
    {
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            Debug.Log(string.Format("Device found with name '{0}' and role '{1}'", device.name, device.role.ToString()));
        }

        var gameControllers = new List<InputDevice>();
        InputDevices.GetDevicesWithRole(InputDeviceRole.GameController, gameControllers);

        foreach (var device in gameControllers)
        {
            Debug.Log(string.Format("Device name '{0}' has role '{1}'", device.name, device.role.ToString()));
        }

        var rightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

        if (rightHandDevices.Count == 1)
        {
            InputDevice device = rightHandDevices[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.role.ToString()));
        }
        else if (rightHandDevices.Count > 1)
        {
            Debug.Log("Found more than one left hand!");
        }
        rightController = rightHandDevices[0];
    }

    // Update is called once per frame
    void Update()
    {
        bool triggerValue;
        drawing = false;
        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue)
        {
            drawing = true;
            //Debug.Log("Trigger button is pressed");
        }
        bool gripValue;
        inserting = false;
        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripValue) && gripValue)
        {
            inserting = true;
            //Debug.Log("Grip button is pressed");
        }
        bool insertValue;
        clearing = false;
        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out insertValue) && insertValue)
        {
            clearing = true;
            //Debug.Log("A button is pressed");
        }
        bool repeatValue;
        repeating = false;
        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out repeatValue) && repeatValue)
        {
            repeating = true;
            //Debug.Log("B button is pressed");
        }
    }
}
