using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ShapeGrammarGenerator
{
    public class Generator : MonoBehaviour
    {
        string commandOutput = "";
        MinizincSolver solver;

        private void OnGUI()
        {
            if (GUI.Button(new Rect(20, 40, 80, 20), "Run cmd"))
            {
                commandOutput = solver.Run();
            }

            GUI.Label(new Rect(20, 80, 800, 200), commandOutput);
        }

        // Start is called before the first frame update
        void Start()
        {
            solver = new MinizincSolver(@"C:\Users\tomka\Desktop\ContentGeneration\ContentGeneration\Assets\ShapeGrammarGenerator\Minizinc\TopologySolver1D.mzn", "");
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    class MinizincSolver
    {
        string programPath;
        string dataPath;

        public MinizincSolver(string programPath, string dataPath)
        {
            this.programPath = programPath;
            this.dataPath = dataPath;
        }

        public string Run()
        {
            string arguments;
            arguments = $"{programPath} {dataPath}";
            var p = new Process();
            p.StartInfo.FileName = @"C:\Program Files\MiniZinc\minizinc.exe";
            p.StartInfo.Arguments = arguments;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            var commandOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return commandOutput;
        }
    }
}
