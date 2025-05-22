using Fusion;
using UnityEngine;

namespace CodeBase.Services.InputService
{
    [System.Serializable]
    public struct NetworkInputData : INetworkInput
    {
        public float MoveX;
        public float MoveY;
        public bool Jump;

        // Свойство для удобства работы с вектором движения
        public Vector2 MoveInput 
        { 
            get => new Vector2(MoveX, MoveY); 
            set { MoveX = value.x; MoveY = value.y; }
        }
    }
}