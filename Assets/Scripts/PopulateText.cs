using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HoloTask
{
    private String taskName;
    private int numSteps;
    private int currStep;
    private Dictionary<int, System.Object> map;
    private bool active;
    private Dictionary<int, string> names;
    private string stepSuffix = ".";
    public HoloTask(String name, int steps)
    {
        taskName = name;
        numSteps = steps;
        currStep = 0;
        map = new Dictionary<int, object>();
        active = false;
        names = new Dictionary<int, string>();
        names.Add(0, "preprocessing stage");
    }
    public void Begin()
    {
        if (map.Count < numSteps || map.Count == 0 || numSteps == 0)
        {
            Console.WriteLine("[HLU] ERROR: Not all steps have a proper key, or there are an invalid number of steps!");
            return;
        }
        currStep = 1;
        active = true;
    }

    public bool Check(System.Object o)
    {
        if (!active)
        {
            Console.WriteLine("[HLU] Task not initialized [use .Begin()]");
            return false;
        }

        if (map[currStep].Equals(o))
        {
            currStep++;
            return true;
        }

        return false;
    }

    public void SetStepObjective(int step, System.Object goal)
    {
        if (step > numSteps)
        {
            Console.WriteLine("[HLU] ERROR: Attempted to set step out of maximum bounds (" + step + " is greater than " + numSteps + ")");
            return;
        }
        map[step] = goal;
    }

    /*
    addDynamicStep(Object goal) 

    This will add a new step at currStep+1 with the goal goal.
    This will also shift all goals after that +1
     */

    public void AddDynamicStep(System.Object goal)
    {
        numSteps++;
        int newStep = currStep++;
        Dictionary<int, System.Object> mapClone = new Dictionary<int, object>();
        for (int i = 0; i < newStep; i++)
        {
            if (i == newStep)
            {
                mapClone[newStep] = goal;
            }
            else if (i > newStep)
            {
                mapClone[i] = map[i - 1];
            }
            else
            {
                mapClone[i] = map[i];
            }
        }

        map = mapClone;
    }

    public string GetSynchronusURLGoal(string url)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        string responseData = string.Empty;
        using (Stream responseStream = response.GetResponseStream())
        {
            if (responseStream != null)
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseData = reader.ReadToEnd();
                }
            }
        }
        return responseData;
    }

    public async Task<string> GetAsyncURLGoal(string url)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";

        HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
        string responseData = string.Empty;
        using (Stream responseStream = response.GetResponseStream())
        {
            if (responseStream != null)
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseData = await reader.ReadToEndAsync();
                }
            }
        }
        return responseData;
    }

    public string PostSynchronusURLGoal(string url, string postData)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";

        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        request.ContentType = "application/json";
        request.ContentLength = byteArray.Length;

        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(byteArray, 0, byteArray.Length);
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        string responseData = string.Empty;
        using (Stream responseStream = response.GetResponseStream())
        {
            if (responseStream != null)
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseData = reader.ReadToEnd();
                }
            }
        }
        return responseData;
    }

    public async Task<string> PostAsyncURLGoal(string url, string postData)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";

        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        request.ContentType = "application/json";
        request.ContentLength = byteArray.Length;

        using (Stream requestStream = await request.GetRequestStreamAsync())
        {
            await requestStream.WriteAsync(byteArray, 0, byteArray.Length);
        }

        HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
        string responseData = string.Empty;
        using (Stream responseStream = response.GetResponseStream())
        {
            if (responseStream != null)
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseData = await reader.ReadToEndAsync();
                }
            }
        }
        return responseData;
    }

    public void SetStepName(int step, string name)
    {
        if (names.ContainsKey(step))
        {
            names.Remove(step);
        }

        names.Add(step, name);
    }

    public string GetStepName(int step, bool ShowStepNumber = false) // optional param to include the step number in the response
    {
        if (names.ContainsKey(step))
        {
            return ShowStepNumber ? step.ToString() + stepSuffix + " " + names[step] : names[step];
        }
        else
        {
            Console.WriteLine("[HLU] ERROR: Attempted to get step name that was not set! (Step " + step + ")");
            return null;
        }
    }

    public List<string> GetAllStepNames(bool ShowStepNumber = false)
    {
        List<string> ret = new List<string>();
        if (names.Count - 1 == numSteps) // -1 to account for the step 0
        {
            for (int i = 1; i < names.Count; i++) // we can do < here instead of <= because we dont use step 0, thus making names.Count 1 higher than we actually need
            {
                ret.Add(ShowStepNumber ? i.ToString() + stepSuffix + " " + names[i] : names[i]);
            }
            return ret;
        }
        else
        {
            for (int i = 1; i <= numSteps; i++) // we have to do <= here instead of just < because numSteps is our exact step count
            {
                if (names.ContainsKey(i))
                {
                    ret.Add(ShowStepNumber ? i.ToString() + stepSuffix + " " + names[i] : names[i]);
                }
                else
                {
                    ret.Add(ShowStepNumber ? i.ToString() + stepSuffix + " No name set" : "No name set");
                }
            }
            return ret;
        }

    }

    public void SetAllStepNames(List<string> lit)
    {
        if (lit.Count == numSteps)
        {
            int count = 1;
            foreach (string s in lit)
            {
                SetStepName(count, s);
                count++;
            }
        }
        else
        {
            Console.WriteLine("[HLU] ERROR: SetAllStepNames length is not correct! It must be the same length as you have number of steps");
        }
    }
    public void SetStepSuffix(string s)
    {
        stepSuffix = s;
    }

    public int GetCurrentStepNumber()
    {
        return currStep;
    }

    public bool IsComplete()
    {
        return currStep > numSteps;
    }

}

public class PopulateText : MonoBehaviour
{
    public TMP_Text helperText;
    //holotask sample code
    //HoloTask testTask = new HoloTask("Test", 2);
    HoloTask samplePrintSetup = new HoloTask("Print Setup", 3);
    void Start()
    {
        samplePrintSetup.SetStepName(1, "Preheat the bed to 65 degrees");
        samplePrintSetup.SetStepObjective(1, 65);
        samplePrintSetup.SetStepName(2, "Set the nozzle temperature to 210 degrees");
        samplePrintSetup.SetStepObjective(2, 210);
        samplePrintSetup.SetStepName(3, "Start the print");
        samplePrintSetup.SetStepObjective(3, "PRINTING");

    }
    void Update()
    {
        int currStep = samplePrintSetup.GetCurrentStepNumber();
        //check if we're done yet
        if (samplePrintSetup.IsComplete())
        {
            helperText.text = "All done!";
            return;
        } else {
            helperText.text = samplePrintSetup.GetStepName(currStep);
        }
        //update the helper text with the current step name        
        /*
        probably put code to do the actual printer referencing and seeing the status of whatever we're looking for
        based on the step number here? and then we can check for completion and this will check on the
        printer again and again each frame until it's done basically and continue making sure
        the helper text is set to whatever it's supposed to be set to
        */
        //check on completion
        switch(currStep) {
            case 1:
                samplePrintSetup.Check(samplePrintSetup.GetSynchronusURLGoal("http://10.204.140.12/api/v1/printer/bed/temperature"));
                break;
            case 2:
                samplePrintSetup.Check(samplePrintSetup.GetSynchronusURLGoal("http://10.204.140.12/api/v1/printer/heads/0/extruders/0/hotend/temperature"));
                break;
            case 3:
                samplePrintSetup.Check(samplePrintSetup.GetSynchronusURLGoal("http://10.204.140.12/api/v1/printer/status"));
                break;
        }
        

    }
}
