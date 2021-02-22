#region

using System;
using System.Threading.Tasks;
using Darkages.Network.ServerFormats;
using Darkages.Types;

#endregion

namespace Darkages.Scripting.Scripts.Skills
{
    [Script("Dash", "Dom")]
    public class Dash : SkillScript
    {
        public Skill _skill;
        public Sprite Target;
        private readonly Random rand = new Random();

        public Dash(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.SendMessage(0x02,
                    string.IsNullOrEmpty(Skill.Template.FailMessage) ? Skill.Template.FailMessage : "failed.");
            }
        }

        public override async void OnSuccess(Sprite sprite)
        {
            var collided = false;

            var action = new ServerFormat1A
            {
                Serial = sprite.Serial,
                Number = 0x82,
                Speed = 20
            };

            for (var i = 0; i < 10; i++) {
                if (sprite is Aisling aisling)
                {
                    //var position = aisling.Client.Position;
                    var position = aisling.Position;

                    if (sprite.Direction == 0)
                        position.Y--;
                    if (sprite.Direction == 1)
                        position.X++;
                    if (sprite.Direction == 2)
                        position.Y++;
                    if (sprite.Direction == 3)
                        position.X--;

                    if (!aisling.Client.Aisling.Map.IsWall(position.X, position.Y))
                        aisling.Client.WarpTo(position);
                }
            }

            await Task.Delay(50).ContinueWith(dc =>
            {
                if (Target != null && collided)
                {
                    if (Target is Monster || Target is Mundane || Target is Aisling)
                        Target.Show(Scope.NearbyAislings,
                            new ServerFormat29((uint)sprite.Serial, (uint)Target.Serial,
                                Skill.Template.TargetAnimation, 0, 100));

                    sprite.Show(Scope.NearbyAislings, action);
                }
            }).ConfigureAwait(true);
        }

        public override void OnUse(Sprite sprite)
        {
            if (!Skill.Ready)
                return;

            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                if (Skill.Ready)
                {
                    if (client.Aisling.Invisible && Skill.Template.PostQualifers.HasFlag(PostQualifer.BreakInvisible))
                    {
                        client.Aisling.Invisible = false;
                        client.Refresh();
                    }

                    client.TrainSkill(Skill);
                }
            }

            //var success = Skill.RollDice(rand);

            //if (success)
                OnSuccess(sprite);
            //else
            //    OnFailed(sprite);
        }
    }
}