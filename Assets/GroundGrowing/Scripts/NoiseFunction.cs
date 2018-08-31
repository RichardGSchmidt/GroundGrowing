using System.Collections;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;


using UnityEngine;

public enum FilterType{Clamp,Invert,Turbulence,Translate,Terrace,ScaleBias,Scale,Rotate,ABS,Exponent}

public class NoiseFunction
{
    public enum BlendMode { Add, Subtract, Max, Min, Multiply, Power };

    /// <summary>
    /// Links
    /// Max
    /// min
    /// multiply
    /// power
    /// </summary>

    public enum NoiseType { Perlin, Billow, RidgedMultifractal, Voronoi, None };
    //[Range(0,1)]
    //public float noiseScale = 0.5f;
    public NoiseType type = NoiseType.Perlin;
    public bool enabled = false;
    public NoiseFunction noiseChild;
    public NoiseFunction noiseParent = null;
    public NoiseFilter[] linkedFilters = null;

    [Range(0f, 20f)]
    public double frequency;
    [Range(2.0000000f, 2.5000000f)]
    public double lacunarity;
    [Range(0f, 1f)]
    public double persistence;
    [Range(1, 18)]
    public int octaves;
    public int seed;
    public BlendMode Blend { get; set; }
    public QualityMode qualityMode;

    public double displacement;
    public bool distance;

    public NoiseFunction()
    {
        GetDefault();
    }

    public void AddChild()
    {
        if (noiseChild == null)
        {
            noiseChild = new NoiseFunction
            {
                noiseParent = this
            };
        }
        else noiseChild.AddChild();
    }

    public void AddChild(NoiseFunction noiseIn)
    {
        if (noiseChild == null)
        {
            noiseChild = noiseIn;
            noiseChild.noiseParent = this;
        }

        else noiseChild.AddChild(noiseIn);
    }

    public void AddChild(NoisePresets presetIn)
    {
        if (noiseChild == null)
        {
            noiseChild = new NoiseFunction(presetIn)
            {
                noiseParent = this
            };
        }

        else noiseChild.AddChild(presetIn);
    }

    public void AddChild(NoiseType _noiseType, bool _enabled, double _frequency, double _lacunarity, double _persistance, int _octaves, QualityMode _qMode, double _displacement, bool _distance, BlendMode _blendmode)
    {

        if (noiseChild == null)
        {
            noiseChild = new NoiseFunction();
            noiseChild.type = _noiseType;
            noiseChild.enabled = _enabled;
            noiseChild.frequency = _frequency;
            noiseChild.lacunarity = _lacunarity;
            noiseChild.persistence = _persistance;
            noiseChild.octaves = _octaves;
            noiseChild.qualityMode = _qMode;
            noiseChild.displacement = _displacement;
            noiseChild.distance = _distance;
            noiseChild.Blend = _blendmode;
            noiseChild.noiseParent = this;
        }
        else noiseChild.AddChild(_noiseType, _enabled, _frequency, _lacunarity, _persistance, _octaves, _qMode, _displacement, _distance, _blendmode);
    }

    public void RemoveChild()
    {
        if (noiseChild.noiseChild != null)
        {
            NoiseFunction temp = noiseChild.noiseChild;
            this.noiseChild = null;
            this.AddChild(temp);
        }
        else this.noiseChild = null;

    }


    public NoiseFunction RemoveSelf(NoiseFunction toRemove)
    {
        NoiseFunction tempNoise = toRemove;

        if (toRemove.noiseChild != null)
        {
            if(toRemove.noiseParent != null)
            {
                tempNoise.noiseParent.noiseChild = toRemove.noiseChild;
            }
            tempNoise.noiseChild.noiseParent = toRemove.noiseParent;
            
            return GetTopMember(tempNoise);
        }
        else if (toRemove.noiseParent != null)
        {
            tempNoise = toRemove;
            tempNoise.noiseParent.noiseChild = null;
            return GetTopMember(tempNoise);
        }
        return null;
    }

    public NoiseFunction GetTopMember(NoiseFunction _input)
    {
        if (_input.noiseParent != null)
        {
            _input = GetTopMember(_input.noiseParent);
        }
        return _input;
    }

    public int GetCount (NoiseFunction _input, int previousCount)
    {
        int counter = previousCount;
        if (_input == null)
        {
            return counter;
        }
        else counter++;

        if (_input.noiseChild != null)
        {
            counter = GetCount(_input.noiseChild, counter);

        }
        return counter;

    }

    public int GetCount(NoiseFunction _input)
    {
        int counter = 0;
        if (_input == null)
        {
            return counter;
        }
        else counter++;

        if (_input.noiseChild !=null)
        {
            counter = GetCount(_input.noiseChild, counter);

        }
        return counter;
    }

    public void GetDefault()
    {
        type = NoiseType.Perlin;
        enabled = true;
        frequency = 1.1;
        lacunarity = 2.2;
        persistence = 0.6;
        octaves = 7;
        qualityMode = QualityMode.Low;
        displacement = 1;
        distance = true;
        Blend = BlendMode.Add;
    }

    public void AttachFilter(NoiseFilter _filter)
    {
        if (linkedFilters == null)
        {
            linkedFilters = new NoiseFilter[1];
            linkedFilters[0] = _filter;
        }
        else
        {
            NoiseFilter[] placeHolder;
            placeHolder = new NoiseFilter[linkedFilters.Length + 1];
            for (int i = 0; i < linkedFilters.Length; i++)
            {
                placeHolder[i] = linkedFilters[i];
            }
            placeHolder[placeHolder.Length - 1] = _filter;
            linkedFilters = placeHolder;
            _filter.FilterIndex = placeHolder.Length-1;
            _filter.AttachToBase(this);
        }
    }
    
    public ModuleBase MakeNoise()
    {
        //this function builds the base module out of the noise function that calls it, lets make it recursive
        ModuleBase _baseModule = null;
        ModuleBase _childBase = null;
        if (type == NoiseType.Billow) { _baseModule = new Billow(frequency, lacunarity, persistence, octaves, seed, qualityMode); }
        else if (type == NoiseType.Perlin) { _baseModule = new Perlin(frequency, lacunarity, persistence, octaves, seed, qualityMode); }
        else if (type == NoiseType.Voronoi) { _baseModule = new Voronoi(frequency, displacement, seed, distance); }
        else if (type == NoiseType.RidgedMultifractal) { _baseModule = new RidgedMultifractal(frequency, lacunarity, octaves, seed, qualityMode); }
        if (noiseChild != null)
        {
            Debug.Log("making child base module");
            _childBase = noiseChild.MakeNoise();
                                    

            if (noiseChild.Blend == NoiseFunction.BlendMode.Power)
            {
                _baseModule = new Power(_baseModule, _childBase);
            }
            else if (noiseChild.Blend == NoiseFunction.BlendMode.Subtract)
            {
                Debug.Log("subtracting");
                _baseModule = new Subtract(_baseModule, _childBase);
            }
            else if (noiseChild.Blend == NoiseFunction.BlendMode.Max)
            {
                _baseModule = new Max(_baseModule, _childBase);
            }
            else if (noiseChild.Blend == NoiseFunction.BlendMode.Min)
            {
                _baseModule = new Min(_baseModule, _childBase);
            }
            else if (noiseChild.Blend == NoiseFunction.BlendMode.Multiply)
            {
                _baseModule = new Multiply(_baseModule, _childBase);
            }
            else
            {
                Debug.Log("adding");
                _baseModule = new Add(_baseModule, _childBase);
            }
            
        }

        
        
        return _baseModule;
        
    }

    #region Noise Functions Preset Handling

    public NoiseFunction(NoiseFunction intakeNoise)
    {
        noiseChild = intakeNoise.noiseChild;
        enabled = intakeNoise.enabled;
        frequency = intakeNoise.frequency;
        lacunarity = intakeNoise.lacunarity;
        persistence = intakeNoise.persistence;
        octaves = intakeNoise.octaves;
        qualityMode = intakeNoise.qualityMode;
        type = intakeNoise.type;
        Blend = intakeNoise.Blend;
        displacement = intakeNoise.displacement;
        distance = intakeNoise.distance;
    }

    public NoiseFunction(NoisePresets presets)
    {
        enabled = presets.enabled;
        frequency = presets.frequency;
        lacunarity = presets.lacunarity;
        persistence = presets.persistence;
        octaves = presets.octaves;
        if (presets.qualityMode == NoisePresets.QualityMode.High)
        {
            qualityMode = QualityMode.High;
        }
        else if (presets.qualityMode == NoisePresets.QualityMode.Medium)
        {
            qualityMode = QualityMode.Medium;
        }
        else
        {
            qualityMode = QualityMode.Low;
        }

        if (presets.noiseType == NoisePresets.NoiseType.Billow)
        {
            type = NoiseType.Billow;
        }
        else if (presets.noiseType == NoisePresets.NoiseType.Perlin)
        {
            type = NoiseType.Perlin;
        }
        else if (presets.noiseType == NoisePresets.NoiseType.RidgedMultifractal)
        {
            type = NoiseType.RidgedMultifractal;
        }
        else if (presets.noiseType == NoisePresets.NoiseType.Voronoi)
        {
            type = NoiseType.Voronoi;
        }
        else
        {
            type = NoiseType.None;
        }

        if (presets.blendMode == NoisePresets.BlendMode.Subtract)
        {
            Blend = BlendMode.Subtract;
        }
        else if (presets.blendMode == NoisePresets.BlendMode.Max)
        {
            Blend = BlendMode.Max;
        }
        else if (presets.blendMode == NoisePresets.BlendMode.Min)
        {
            Blend = BlendMode.Min;
        }
        else if (presets.blendMode == NoisePresets.BlendMode.Multiply)
        {
            Blend = BlendMode.Multiply;
        }
        else if (presets.blendMode == NoisePresets.BlendMode.Power)
        {
            Blend = BlendMode.Power;
        }
        else
        {
            Blend = BlendMode.Add;
        }



        displacement = presets.displacement;
        distance = presets.distance;
    }
    public NoisePresets GetPresets()
    {
        NoisePresets preset = new NoisePresets();
        preset.enabled = enabled;
        preset.frequency = frequency;
        preset.lacunarity = lacunarity;
        preset.persistence = persistence;
        preset.octaves = octaves;
        preset.displacement = displacement;
        preset.distance = distance;


        if (qualityMode == QualityMode.High)
        {
            preset.qualityMode = NoisePresets.QualityMode.High;
        }
        else if (qualityMode == QualityMode.Medium)
        {
            preset.qualityMode = NoisePresets.QualityMode.Medium;
        }
        else
        {
            preset.qualityMode = NoisePresets.QualityMode.Low;
        }

        if (type == NoiseType.Perlin)
        {
            preset.noiseType = NoisePresets.NoiseType.Perlin;
        }
        else if (type == NoiseType.Billow)
        {
            preset.noiseType = NoisePresets.NoiseType.Billow;
        }
        else if (type == NoiseType.RidgedMultifractal)
        {
            preset.noiseType = NoisePresets.NoiseType.RidgedMultifractal;
        }
        else if (type == NoiseType.Voronoi)
        {
            preset.noiseType = NoisePresets.NoiseType.Voronoi;
        }
        else
        {
            preset.noiseType = NoisePresets.NoiseType.None;
        }

        if (Blend == BlendMode.Subtract)
        {
            preset.blendMode = NoisePresets.BlendMode.Subtract;
        }
        else if (Blend == BlendMode.Power)
        {
            preset.blendMode = NoisePresets.BlendMode.Power;
        }
        else if (Blend == BlendMode.Multiply)
        {
            preset.blendMode = NoisePresets.BlendMode.Multiply;
        }
        else if (Blend == BlendMode.Min)
        {
            preset.blendMode = NoisePresets.BlendMode.Min;
        }
        else if (Blend == BlendMode.Max)
        {
            preset.blendMode = NoisePresets.BlendMode.Max;
        }
        else 
        {
            preset.blendMode = NoisePresets.BlendMode.Add;
        }

        return preset;
    }
    #endregion

}

/// <summary>
/// Blend(ModuleBase lhs, ModuleBase rhs, ModuleBase controller)
/// Displace (4) 1 input 3 mutators
/// Select(ModuleBase inputA, ModuleBase inputB, ModuleBase controller)
/// Turbulence
/// </summary>
public abstract class NoiseJoiner
{
    public int mode;
   
    public NoiseFunction[] joinedNoises;
}

/// <summary>

/// Turbulence
/// </summary>
public abstract class NoiseFilter
{
    //this abstract will be attached to the main Note function.
    public FilterType Type { get; set; }
    public NoiseFunction Attached { get; set; }
    public bool FilterEnabled { get; set; }
    public int FilterIndex { get; set; }
    public Rect BaseLocation { get; set; }
    public Rect Box1 { get; set; }
    public Rect Box2 { get; set; }
    public Rect Box3 { get; set; }
    public Rect Box4 { get; set; }
    public Rect Box5 { get; set; }
    public string Name { get; set; }
    public const float yInterval = 20f;

    public NoiseFilter()
    {
        FilterEnabled = false;
        Attached = null;
    }

    public NoiseFilter(ref Rect _boxIn)
    {
        BaseLocation = _boxIn;
        GetLocations();
        FilterEnabled = false;
        Attached = null;
    }

    abstract public ModuleBase RunFilter(ModuleBase _mBase);

    abstract public void GetInspectorElements(ref Rect _box);

    public void GetLocations()
    {
        Box1 = new Rect(BaseLocation.x + 10f, BaseLocation.y + yInterval, 300f, 20f);
        Box2 = new Rect(BaseLocation.x + 10f, BaseLocation.y + 2*yInterval, 300f, 20f);
        Box3 = new Rect(BaseLocation.x + 10f, BaseLocation.y + 3*yInterval, 300f, 20f);
        Box4 = new Rect(BaseLocation.x + 10f, BaseLocation.y + 4*yInterval, 300f, 20f);
        Box5 = new Rect(BaseLocation.x + 10f, BaseLocation.y + 5*yInterval, 300f, 20f);

    }

    public void GetLocations(Rect locIn)
    {
        Box1 = new Rect(locIn.x + 10f, locIn.y + yInterval, 300f, 20f);
        Box2 = new Rect(locIn.x + 10f, locIn.y + 2 * yInterval, 300f, 20f);
        Box3 = new Rect(locIn.x + 10f, locIn.y + 3 * yInterval, 300f, 20f);
        Box4 = new Rect(locIn.x + 10f, locIn.y + 4 * yInterval, 300f, 20f);
        Box5 = new Rect(locIn.x + 10f, locIn.y + 5 * yInterval, 300f, 20f);

    }

    public Rect SetWindowSize(Rect _rectIn)
    {
        return (new Rect(_rectIn.x, _rectIn.y, 400f,200f));    
    }

    public void AttachToBase(NoiseFunction _noiseFunc)
    {
        Attached = _noiseFunc;
    }
}

/// <summary>
/// turbulance needs to be split in two
/// </summary>
public class TurbulenceFilter : NoiseFilter
{
    double Power { get; set; }

    public TurbulenceFilter():base()
    {
        Name = "Turbulence";
        Type = FilterType.Turbulence;
        FilterEnabled = true;
    }

    public TurbulenceFilter(ref Rect _boxIn):base(ref _boxIn)
    {
        Name = "Turbulence";
        Type = FilterType.Turbulence;
        FilterEnabled = true;
    }

    public override ModuleBase RunFilter(ModuleBase _mBase)
    {
        return new Turbulence(Power, _mBase);
    }

    public override void GetInspectorElements(ref Rect _box)
    {
        _box = SetWindowSize(_box);
        GetLocations(_box);
        this.Name = "Turbulence";
        UnityEditor.EditorGUI.LabelField(Box1, Name);
        this.FilterEnabled = UnityEditor.EditorGUI.ToggleLeft(Box2, "Enabled", this.FilterEnabled);
        this.Power = (double)UnityEditor.EditorGUI.Slider(Box3,"Power",(float)this.Power, -2, 10);
    }
}

public class TranslateFilter : NoiseFilter
{
    double X { get; set; }
    double Y { get; set; }
    double Z { get; set; }
    public override ModuleBase RunFilter(ModuleBase _mBase)
    {
        return new Translate(X, Y, Z, _mBase);
    }
    public override void GetInspectorElements(ref Rect _box)
    {
        GetLocations(_box);
        _box = SetWindowSize(_box);
        this.Name = "Translate";
        UnityEditor.EditorGUI.LabelField(Box1, Name);
        this.FilterEnabled = UnityEditor.EditorGUI.ToggleLeft(Box2, "Enabled", this.FilterEnabled);
        this.X = (double)UnityEditor.EditorGUI.Slider(Box3, "X", (float)this.X, -200, 200);
        this.Y = (double)UnityEditor.EditorGUI.Slider(Box4, "Y", (float)this.Y, -200, 200);
        this.Z = (double)UnityEditor.EditorGUI.Slider(Box5, "Z", (float)this.Z, -200, 200);
        //add linkage checkbox
    }
}

public class TerraceFilter : NoiseFilter
{
    bool Inverted { get; set; }
    public override ModuleBase RunFilter(ModuleBase _mBase)
    {
       return new Terrace(Inverted, _mBase);
    }
    public override void GetInspectorElements(ref Rect _box)
    {
        GetLocations(_box);
        _box = SetWindowSize(_box);
        this.Name = "Terrace";
        UnityEditor.EditorGUI.LabelField(Box1, Name);
        this.FilterEnabled = UnityEditor.EditorGUI.ToggleLeft(Box2, "Enabled", this.FilterEnabled);
        this.Inverted = UnityEditor.EditorGUI.Toggle(Box3, "Inverted", this.Inverted);
    }
}

public class ScaleBiasFilter: NoiseFilter
{
    double _Scale { get; set; }
    double _Bias  { get; set; }
    public override ModuleBase RunFilter(ModuleBase _mBase)
    {
        return new ScaleBias(_Scale, _Bias, _mBase);
    }
    public override void GetInspectorElements(ref Rect _box)
    {
        GetLocations(_box);
        _box = SetWindowSize(_box);
        this.Name = "Scale Bias";
        UnityEditor.EditorGUI.LabelField(Box1, Name);
        this.FilterEnabled = UnityEditor.EditorGUI.ToggleLeft(Box2, "Enabled", this.FilterEnabled);
        this._Scale = (double)UnityEditor.EditorGUI.Slider(Box3, "Scale", (float)this._Scale, -200, 200);
        this._Bias = (double)UnityEditor.EditorGUI.Slider(Box4, "Bias", (float)this._Bias, -200, 200);
    }
}

public class ScaleFilter : NoiseFilter
{
    double X { get; set; }
    double Y { get; set; }
    double Z { get; set; }
    public override ModuleBase RunFilter(ModuleBase _mBase)
    {
        return new Scale(X, Y, Z, _mBase);
    }
    public override void GetInspectorElements(ref Rect _box)
    {
        GetLocations(_box);
        _box = SetWindowSize(_box);
        this.Name = "Scale";
        UnityEditor.EditorGUI.LabelField(Box1, Name);
        this.FilterEnabled = UnityEditor.EditorGUI.ToggleLeft(Box2, "Enabled", this.FilterEnabled);
        this.X = (double)UnityEditor.EditorGUI.Slider(Box3, "X", (float)this.X, -200, 200);
        this.Y = (double)UnityEditor.EditorGUI.Slider(Box4, "Y", (float)this.Y, -200, 200);
        this.Z = (double)UnityEditor.EditorGUI.Slider(Box5, "Z", (float)this.Z, -200, 200);
    }
}

public class RotateFilter : NoiseFilter
{
    double X { get; set; }
    double Y { get; set; }
    double Z { get; set; }
    public override ModuleBase RunFilter(ModuleBase _mBase)
    {
        return new Rotate(X,Y,Z,_mBase);
    }
    public override void GetInspectorElements(ref Rect _box)
    {
        GetLocations(_box);
        _box = SetWindowSize(_box);
        this.Name = "Rotate";
        UnityEditor.EditorGUI.LabelField(Box1, Name);
        this.FilterEnabled = UnityEditor.EditorGUI.ToggleLeft(Box2, "Enabled", this.FilterEnabled);
        this.X = (double)UnityEditor.EditorGUI.Slider(Box3, "X", (float)this.X, -200, 200);
        this.Y = (double)UnityEditor.EditorGUI.Slider(Box4, "Y", (float)this.Y, -200, 200);
        this.Z = (double)UnityEditor.EditorGUI.Slider(Box5, "Z", (float)this.Z, -200, 200);
    }
}

public class ABSFilter : NoiseFilter
{
    public override ModuleBase RunFilter(ModuleBase _mBase)
    {
        return new Abs(_mBase);
    }

    public override void GetInspectorElements(ref Rect _box)
    {
        GetLocations(_box);
        _box = SetWindowSize(_box);
        this.Name = "Absolute Value";
        UnityEditor.EditorGUI.LabelField(Box1, Name);
        this.FilterEnabled = UnityEditor.EditorGUI.ToggleLeft(Box2, "Enabled", this.FilterEnabled);
    }
}

public class ClampFilter : NoiseFilter
{
    double MinValue { get; set; }
    double MaxValue { get; set; }
    
    public override ModuleBase RunFilter(ModuleBase _mBase)
    {
        return new Clamp(MinValue,MaxValue,_mBase);
    }

    public override void GetInspectorElements(ref Rect _box)
    {
        GetLocations(_box);
        _box = SetWindowSize(_box);
        this.Name = "Clamp";
        UnityEditor.EditorGUI.LabelField(Box1, Name);
        this.FilterEnabled = UnityEditor.EditorGUI.ToggleLeft(Box2, "Enabled", this.FilterEnabled);
        this.MinValue = (double)UnityEditor.EditorGUI.Slider(Box3, "Min", (float)this.MinValue, -3, 3);
        this.MaxValue = (double)UnityEditor.EditorGUI.Slider(Box4, "Max", (float)this.MaxValue, -3, 3);
    }
}

public class ExponentFilter : NoiseFilter
{
    double CoEfficient { get; set; }
    public override ModuleBase RunFilter(ModuleBase _mBase)
    {
        return new Exponent(CoEfficient, _mBase);
    }

    public override void GetInspectorElements(ref Rect _box)
    {
        GetLocations(_box);
        _box = SetWindowSize(_box);
        this.Name = "Exponent";
        UnityEditor.EditorGUI.LabelField(Box1, Name);
        this.FilterEnabled = UnityEditor.EditorGUI.ToggleLeft(Box2, "Enabled", this.FilterEnabled);
        this.CoEfficient = (double)UnityEditor.EditorGUI.Slider(Box3, "Power", (float)this.CoEfficient, -2, 10);
    }
}

public class InvertFilter : NoiseFilter
{
    public override ModuleBase RunFilter(ModuleBase _mBase)
    {
        return new Invert( _mBase);
    }

    public override void GetInspectorElements(ref Rect _box)
    {
        GetLocations(_box);
        _box = SetWindowSize(_box);
        this.Name = "Invert";
        UnityEditor.EditorGUI.LabelField(Box1, Name);
        this.FilterEnabled = UnityEditor.EditorGUI.ToggleLeft(Box2, "Enabled", this.FilterEnabled);
    }
}


/// <summary>
/// Curve is going to need it's own custom implementation.
/// </summary>

#region Noise Presets Serializable Container
//used to save an .npr file
[System.Serializable]
public struct NoisePresets
{
    public enum NoiseType { Perlin, Billow, RidgedMultifractal, Voronoi, None };
    public enum QualityMode { Low, Medium, High };
    public enum BlendMode { Add, Subtract, Max, Min, Multiply, Power };
    public NoiseType noiseType;
    public bool enabled;
    public double frequency;
    public double lacunarity;
    public double persistence;
    public int octaves;
    public QualityMode qualityMode;
    public BlendMode blendMode;
    public double displacement;
    public bool distance;

}
#endregion