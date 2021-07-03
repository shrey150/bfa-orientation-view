using UnityEngine;
using Gizmos = Popcron.Gizmos;

public class RotationController : MonoBehaviour
{
    // array containing rotation vectors
    Vector3[] angles;
    Quaternion[] quats;

    // the sample index for the "angles" array
    public int index = 0;

    // indicates animation state
    public bool playing = true;

    // tracks which axes to rotate for (X/Y/Z)
    public bool[] applyRot = { true, true, true };

    private void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-sps":
                    SetSampleRate(int.Parse(args[i + 1]));
                    break;

                case "-file":
                    string data = System.IO.File.ReadAllText(args[i + 1]);
                    LoadTestData(data);
                    GetComponent<UIController>().DisplayArgsMsg();
                    break;

                default:
                    break;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playing = !playing;
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            index = 0;
        }

        // draw x-y-z axis lines
        Gizmos.Line(-Vector3.forward * 5, Vector3.forward * 5, Color.blue, false);
        Gizmos.Line(-Vector3.right * 5, Vector3.right * 5, Color.red, false);
        Gizmos.Line(-Vector3.up * 5, Vector3.up * 5, Color.green, false);
    }

    private void FixedUpdate()
    {
        // if animation is set to playing
        if (playing)
        {
            // quaternions (recommended)
            if (quats != null)
            {
                // apply current quaternion orientation
                transform.rotation = quats[index];
            }
            // Euler angles
            else
            {
                // figure out which axes are enabled for rotation
                float x = applyRot[0] ? angles[index].x : transform.eulerAngles.x;
                float y = applyRot[1] ? angles[index].y : transform.eulerAngles.y;
                float z = applyRot[2] ? angles[index].z : transform.eulerAngles.z;

                // apply current rotation vector
                transform.eulerAngles = new Vector3(x, y, z);
            }

            // iterate to next sample
            index++;
            
            // reset the chosen format's array to 0
            if (angles != null && index == angles.Length) index = 0;
            if (quats != null && index == quats.Length) index = 0;

        }
    }

    public void SetSampleRate(int sampleRate)
    {
        Time.fixedDeltaTime = 1f / sampleRate;
        
        // update input SPS if passed via argument
        GetComponent<UIController>().UpdateInputSPS();
    }

    public int GetSampleRate()
    {
        return Mathf.RoundToInt(1 / Time.fixedDeltaTime);
    }

    public void LoadTestData(string data)
    {
        string[] lines = data.Split('\n');

        // determine whether or not test data uses quats
        bool useQuats = lines[0].Split(',').Length == 4;

        // set the other format's array to null, indicating it's not being used;
        // this will always be mutually exclusive (only one can be null at a time)
        //
        // exclude last line of data set since it's always empty
        angles = !useQuats ? new Vector3[lines.Length - 1] : null;
        quats = useQuats ? new Quaternion[lines.Length - 1] : null;

        for (int i = 0; i < lines.Length - 1; i++)
        {
            string[] samples = lines[i].Split(',');

            // organize into Euler angles
            if (samples.Length == 3)
            {
                // since Unity axes differ from algorithm, adjust for differences
                angles[i] = new Vector3(-float.Parse(samples[1]), float.Parse(samples[2]), float.Parse(samples[0]));
            }
            // organize into quaternions
            else if (samples.Length == 4)
            {

                // TODO: not really sure why this is correct, I just fiddled around w/ values until it worked;
                // it resembles XYZ -> YZX from Euler angle correction, but z term is also negated
                //
                // since Unity axes differ from algorithm, adjust for differences
                quats[i] = new Quaternion(float.Parse(samples[2]), float.Parse(samples[3]), -float.Parse(samples[1]), float.Parse(samples[0]));
            }

        }

        // reset animation to start
        index = 0;
    }

    public void LoadTest(string name)
    {
        LoadTestData(((TextAsset)Resources.Load(name)).text);
        Debug.Log("Loaded test: " + name);
    }
}
