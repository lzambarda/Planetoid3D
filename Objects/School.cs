using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Planetoid3D
{
    [Serializable]
    public class School : BaseObjectBuilding
    {
        public School() { }
        public School(Planet Planet, Matrix initial, int Owner)
        {
            planet = Planet;
            matrix = initial;
            type = BuildingType.School;
            owner = Owner;
            selectedSpecialization = Specialization.Builder;
        }

        public Specialization selectedSpecialization;
        public float learn;
        public Hominid student;
        private BaseObject collider;

        public override string SecondHUDLabel
        {
            get
            {
                return (student!=null ? "Leave Student" : "Change Specialization");
            }
        }

        public override void DoSecondHUDAction()
        {
            if (student==null)
            {
                selectedSpecialization++;
                if (selectedSpecialization > Specialization.Pilot)
                {
                    //"None" specialization is not acceptable
                    selectedSpecialization = Specialization.Builder;
                }
            }
            else
            {
                LeaveHominid();
            }
        }

        public override string GetHudText()
        {
            if (learn > 0)
            {
                string text="Currently training a " +selectedSpecialization + ":";
                if (learn < 10)
                {
                    text += "\nStudent: What??";
                }
                else if (learn < 30)
                {
                    text += "\nStudent: MMM?";
                }
                else if (learn < 50)
                {
                    text += "\nStudent: So.. huh.. yes..";
                }
                else if (learn < 70)
                {
                    text += "\nStudent: Ah-ha oh, ok...";
                }
                else if (learn < 90)
                {
                    text += "\nStudent: Understanding everything!";
                }
                else
                {
                    text += "\nStudent: Going for the promotion!";
                }
                return text + base.GetHudText();
            }
            return "School for " + selectedSpecialization + "s is currently empty."+base.GetHudText();
        }

        public override void InSerialization()
        {
            if (student != null)
            {
                student.InSerialization();
            }
            base.InSerialization();
        }

        public override void OutSerialization()
        {
            base.OutSerialization();
            if (student != null)
            {
                student.OutSerialization();
            }
        }

        public override bool Update(float elapsed)
        {
            if (student==null)
            {
                GetCollidingObject(15,ref collider);
                //Begin student training
                if (collider != null && collider is Hominid && ((Hominid)collider).owner==owner && ((Hominid)collider).specialization==Specialization.Normal)
                {
                    learn = 0;
                    student = ((Hominid)collider);
                    HominidEnteredBuilding((Hominid)collider);
                }
            }
            else
            {
                //If the learning timer is gone
                if (learn < 100)
                {
                    learn += (elapsed*(2.5f+PlayerManager.players[owner].researchLevels[2]/2f));
                }
                else
                {
                    //Finish student training
                    LeaveHominid();
                    planet.hominids.Last().specialization = selectedSpecialization;
                    if (selectedSpecialization == Specialization.Soldier && owner==0)
                    {
                        QuestManager.QuestCall(9);
                    }
                    learn = 0;
                }
            }
            return base.Update(elapsed);
        }

        public override bool LeaveHominid()
        {
            if (student!=null)
            {
                student.matrix = GetMatrix(9, matrix.Right * 2 + matrix.Backward/2f);
                student.specialization = Specialization.Normal;
                student.flying = true;
                student.speed = Vector3.Zero;
                planet.hominids.Add(student);
                student=null;
                learn = 0;
                return false;
            }
            return true;
        }
    }
}
