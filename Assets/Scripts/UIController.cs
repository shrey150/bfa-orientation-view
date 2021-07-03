using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    RotationController rot;

    Text timerText;
    Dropdown chooseTest;
    InputField inputSps;

    // called before the first frame update
    void Awake()
    {
        // fetch rotation controller
        rot = GetComponent<RotationController>();

        // find all apply rotation checkboxes
        GameObject checkboxParent = GameObject.FindGameObjectWithTag("ApplyRot");
        Toggle[] checkboxes = checkboxParent.transform.GetComponentsInChildren<Toggle>();

        // set listeners for each checkbox
        for (int i = 0; i < checkboxes.Length; i++)
        {
            int axis = i;

            checkboxes[i].onValueChanged.AddListener(delegate {
                rot.applyRot[axis] = checkboxes[axis].isOn;
            });
        }

        // find timer text and "choose test" dropdown
        timerText = GameObject.FindGameObjectWithTag("Timer").GetComponent<Text>();
        chooseTest = GameObject.FindGameObjectWithTag("ChooseTest").GetComponent<Dropdown>();

        // find "Sample Rate" input field and make it auto-update
        inputSps = GameObject.FindGameObjectWithTag("SampleRate").GetComponent<InputField>();
        inputSps.onValueChanged.AddListener(delegate {
            rot.SetSampleRate(int.Parse(inputSps.text));
        });

        // load all CSVs from Resources folder
        object[] tests = Resources.LoadAll("", typeof(TextAsset));

        List<Dropdown.OptionData> testNames = new List<Dropdown.OptionData>();

        // populate "choose test" dropdown
        for (int i = 0; i < tests.Length; i++)
        {
            testNames.Add(new Dropdown.OptionData(((TextAsset)tests[i]).name));
        }

        chooseTest.ClearOptions();
        chooseTest.AddOptions(testNames);

        // load test when option is selected in dropdown
        chooseTest.onValueChanged.AddListener(delegate {
            rot.LoadTest(((TextAsset)tests[chooseTest.value]).name);
        });

        // load first test by default
        rot.LoadTest(((TextAsset)tests[0]).name);
    }

    // Update is called once per frame
    void Update()
    {
        // calculate current time from sample rate
        float time = (float) rot.index / rot.GetSampleRate();
        timerText.text = time.ToString("0.00") + "s";
    }

    public void UpdateInputSPS()
    {
        inputSps.text = rot.GetSampleRate().ToString();
    }

    public void DisplayArgsMsg()
    {
        GameObject.FindGameObjectWithTag("Args").GetComponent<Text>().text = "(Loaded from args)";
    }
}
