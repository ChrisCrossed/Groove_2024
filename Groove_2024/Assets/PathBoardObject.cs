using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct PathBoardObject
{
    private Vector2Int _thisPosition;
    private bool _leftValid;
    private bool _rightValid;
    private bool _upValid;
    private bool _downValid;

    public PathBoardObject(Vector2Int position, bool leftValid, bool rightValid, bool upValid, bool downValid)
    {
        _thisPosition = position;
        _leftValid = leftValid;
        _rightValid = rightValid;
        _upValid = upValid;
        _downValid = downValid;
    }

    public bool LeftValid
    {
        get => _leftValid;
        set => _leftValid = value;
    }

    public bool RightValid
    {
        get => _rightValid;
        set => _rightValid = value;
    }

    public bool DownValid
    {
        get => _downValid;
        set => _downValid = value;
    }

    public bool UpValid
    {
        get => _upValid;
        set => _upValid = value;
    }
}
