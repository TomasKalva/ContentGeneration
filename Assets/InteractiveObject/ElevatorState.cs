using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.InteractiveObject
{
    public class ElevatorState : ObjectState
    {
        public bool Up { get; private set; }

        public float Height { get; }

        Elevator Elevator { get; set; } 

        public ElevatorState(float height, bool up)
        {
            Up = up;
            Height = height;
        }

        protected override void AfterObjectSet()
        {
            Elevator = Object.GetComponent<Elevator>();
            Elevator.Max_height = Height;
            Elevator.SetIsUp(Up);
        }

        public void Activate()
        {
            Elevator?.Move();
        }
    }
}
