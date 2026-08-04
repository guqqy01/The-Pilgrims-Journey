using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Input
{
    

    internal class UnityInputManager
    {
        //string yamlPath;
        readonly TextReader? reader;
        InputManager? inputManager;

        public UnityInputManager(string? yamlPath = null, string? text = null)
        {
            //var yamlPath = path.Substring(0, Math.Max(path.LastIndexOf(@"\Assets\"), path.LastIndexOf(@"/Assets/"))) + @"/ProjectSettings/InputManager.asset";

            //if (enchaterPath != null)
                //yamlPath = enchaterPath.Substring(0, Math.Max(enchaterPath.LastIndexOf(@"\Assets\"), enchaterPath.LastIndexOf(@"/Assets/"))) + @"/ProjectSettings/InputManager.asset";

            if (yamlPath != null)
                reader = new StreamReader(yamlPath);

            if (text != null)
                reader = new StringReader(text);

            Read();
        }


        void Read()
        {
            if (reader == null)
            {
                Serilog.Log.Warning($"{nameof(UnityInputManager)}.{nameof(reader)} is null");
                return;
            }

            //var reader = new StreamReader(yamlPath);
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithTagMapping("tag:unity3d.com,2011:13", typeof(Class13Mapper))
                .IgnoreUnmatchedProperties()
                .Build();
            //serializer.Settings.RegisterTagMapping("tag:unity3d.com,2011:13", typeof(Class13));
            //serializer.Settings.ComparerForKeySorting = null;
            Class13Mapper? result;
            try
            {
                result = deserializer.Deserialize<Class13Mapper>(reader);
            }
            catch (YamlDotNet.Core.SemanticErrorException semanticErrorException)
            {
                Serilog.Log.Debug(semanticErrorException, "Couldn't parse InputManager.asset yaml file");
                return;
            }
            finally
            {
                reader.Close();
            }
            var inputManagerMapper = result?.InputManager;
            if (inputManagerMapper == null)
            {
                Serilog.Log.Warning($"{nameof(inputManagerMapper)} is null");
                return;
            }

            inputManager = new InputManager(inputManagerMapper);
        }


        public void SendData()
        {
            if (inputManager == null)
                return;

            var axisNames = inputManager.Axes.Select(a => a.Name!).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToArray();
            var axisInfos = axisNames.Select(a => inputManager.Axes.GetInfo(a)).ToArray();
            CreateBindingsMap(out var buttonKeys, out var buttonAxis);

            NetMQInitializer.Publisher?.SendInputManager(
                axisNames, axisInfos, buttonKeys!, buttonAxis!,
                UnityEngine.Input.GetJoystickNames()
                );


            /*
            NetMQInitializer.Publisher?.SendInputManager(
                inputManager.Axes.Select(a => a.Name).Distinct().ToArray(),
                inputManager.Axes.Select(a => a.positiveButton).ToArray(),
                inputManager.Axes.Select(a => a.negativeButton).ToArray(),
                inputManager.Axes.Select(a => a.altPositiveButton).ToArray(),
                inputManager.Axes.Select(a => a.altNegativeButton).ToArray(),
                UnityEngine.Input.GetJoystickNames()
                );
            */

        }


        void CreateBindingsMap(out string[]? inputKeys, out string[]? inputAxis)
        {
            if (inputManager == null)
            {
                inputKeys = null;
                inputAxis = null;
                return;
            }

            var dict = new Dictionary<string, string?>();

            foreach (var axis in inputManager.Axes)
            {
                if (axis.altNegativeButton != null && !string.IsNullOrEmpty(axis.altNegativeButton))
                    dict[axis.altNegativeButton] = axis.Name;
            }
            foreach (var axis in inputManager.Axes)
            {
                if (axis.negativeButton != null && !string.IsNullOrEmpty(axis.negativeButton))
                    dict[axis.negativeButton] = axis.Name;
            }
            foreach (var axis in inputManager.Axes)
            {
                if (axis.altPositiveButton != null && !string.IsNullOrEmpty(axis.altPositiveButton))
                    dict[axis.altPositiveButton] = axis.Name;
            }
            foreach (var axis in inputManager.Axes)
            {
                if (axis.positiveButton != null && !string.IsNullOrEmpty(axis.positiveButton))
                    dict[axis.positiveButton] = axis.Name;
            }

            var keys = new string[dict.Count];
            var values = new string[dict.Count];
            dict.Keys.CopyTo(keys, 0);
            dict.Values.CopyTo(values, 0);

            inputKeys = keys;
            inputAxis = values;
        }
    }

    public enum AxisType
    {
        KeyOrMouseButton = 0,
        MouseMovement = 1,
        JoystickAxis = 2
    };

#pragma warning disable IDE1006

    public class InputAxisMapper
    {
        public int serializedVersion { get; set; }

        public string? m_Name { get; set; }
        public string? descriptiveName { get; set; }
        public string? descriptiveNegativeName { get; set; }
        public string? negativeButton { get; set; }
        public string? positiveButton { get; set; }
        public string? altNegativeButton { get; set; }
        public string? altPositiveButton { get; set; }

        //public float gravity { get; set; }
        //public float dead { get; set; }
        //public float sensitivity { get; set; }
        public string? gravity { get; set; }
        public string? dead { get; set; }
        public string? sensitivity { get; set; }

        //public bool snap { get; set; }
        public int snap { get; set; }
        //public bool invert { get; set; }
        public int invert { get; set; }

        //public AxisType type { get; set; }
        public int type { get; set; }

        public int axis { get; set; }
        public int joyNum { get; set; }
    }

    public class InputAxis
    {
        readonly InputAxisMapper map;

        public InputAxis(InputAxisMapper map)
        {
            this.map = map;
        }

        public int SerializedVersion
        {
            get { return map.serializedVersion; }
            set { map.serializedVersion = value; }
        }

        public string? Name => map.m_Name;
        public string? descriptiveName => map.descriptiveName;
        public string? descriptiveNegativeName => map.descriptiveNegativeName;
        public string? negativeButton => map.negativeButton;
        public string? positiveButton => map.positiveButton;
        public string? altNegativeButton => map.altNegativeButton;
        public string? altPositiveButton => map.altPositiveButton;

        public float gravity => float.Parse(map.gravity);//**--format
        public float dead => float.Parse(map.dead);//**--format
        public float sensitivity => float.Parse(map.sensitivity);//**--format

        public bool snap => map.snap != 0;
        public bool invert => map.invert != 0;

        public AxisType type => (AxisType)map.type;

        public int axis => map.axis;
        public int joyNum => map.joyNum;
    }

    public class InputManagerMapper
    {
        public int m_ObjectHideFlags { get; set; }
        public int serializedVersion { get; set; }
        public int m_UsePhysicalKeys { get; set; }
        public List<InputAxisMapper>? m_Axes { get; set; }
    }

#pragma warning restore IDE1006

    public class InputManager
    {
        readonly InputManagerMapper map;
        readonly List<InputAxis> axes;

        public InputManager(InputManagerMapper map)
        {
            this.map = map;
            this.axes = new List<InputAxis>();

            if (map.m_Axes == null)
            {
                Serilog.Log.Warning($"map.m_Axes is null");
                return;
            }

            foreach (var a in map.m_Axes)
                this.axes.Add(new InputAxis(a));
        }

        public int ObjectHideFlags
        {
            get { return map.m_ObjectHideFlags; }
            set { map.m_ObjectHideFlags = value; }
        }

        public int SerializedVersion
        {
            get { return map.serializedVersion; }
            set { map.serializedVersion = value; }
        }

        public bool UsePhysicalKeys
        {
            get { return map.m_UsePhysicalKeys != 0; }
            set { map.m_UsePhysicalKeys = value ? 1 : 0; }
        }

        /*public List<InputAxisMapper> Axes
        {
            get { return map.m_Axes; }
            set { map.m_Axes = value; }
        }*/
        public List<InputAxis> Axes => axes;
    }

    public class Class13Mapper
    {
        public InputManagerMapper? InputManager { get; set; }
    }
}
