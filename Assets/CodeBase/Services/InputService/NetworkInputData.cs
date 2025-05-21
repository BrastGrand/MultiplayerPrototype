using Fusion;
using UnityEngine;

namespace CodeBase.Services.InputService
{
    // Для работы с Fusion структура должна быть unmanaged типом
    [System.Serializable]
    public struct NetworkInputData : INetworkInput
    {
        // Оба поля должны быть unmanaged типами (простыми типами)
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