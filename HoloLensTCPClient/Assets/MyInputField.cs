using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class MyInputField: InputField
{

    public event Action InputFieldSelected;

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        InputFieldSelected();

    }


}