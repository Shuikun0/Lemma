﻿using System;
using ComponentBind;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lemma.Components;

namespace Lemma.Factories
{
	public class RiftFactory : Factory<Main>
	{
		public RiftFactory()
		{
			this.Color = new Vector3(0.4f, 1.0f, 0.4f);
		}

		public override Entity Create(Main main)
		{
			Entity entity = new Entity(main, "Rift");
			Rift rift = new Rift();
			rift.Enabled.Value = false;
			entity.Add("Rift", rift);
			PlayerTrigger trigger = new PlayerTrigger();
			trigger.Enabled.Value = false;
			trigger.Radius.Value = 0;
			entity.Add("PlayerTrigger", trigger);
			return entity;
		}

		public override void Bind(Entity entity, Main main, bool creating = false)
		{
			Transform transform = entity.GetOrCreate<Transform>("Position");
			Rift rift = entity.GetOrCreate<Rift>("Rift");
			PlayerTrigger trigger = entity.GetOrCreate<PlayerTrigger>("PlayerTrigger");
			this.SetMain(entity, main);

			Property<Matrix> targetTransform = new Property<Matrix>();
			VoxelAttachable.MakeAttachable(entity, main, false, false, null);
			VoxelAttachable.BindTarget(entity, rift.Position);

			Property<Entity.Handle> voxel = entity.GetOrMakeProperty<Entity.Handle>("AttachedVoxel");
			Property<Voxel.Coord> coord = entity.GetOrMakeProperty<Voxel.Coord>("AttachedCoordinate");

			PointLight light = entity.GetOrCreate<PointLight>();
			light.Color.Value = new Vector3(1.2f, 1.4f, 1.6f);
			light.Add(new Binding<Vector3>(light.Position, rift.Position));
			light.Add(new Binding<bool>(light.Enabled, () => rift.Type == Rift.Style.In && rift.Enabled, rift.Type, rift.Enabled));
			light.Add(new Binding<float>(light.Attenuation, x => x * 2.0f, rift.CurrentRadius));

			rift.Add(new Binding<Entity.Handle>(rift.Voxel, voxel));
			rift.Add(new Binding<Voxel.Coord>(rift.Coordinate, coord));

			trigger.Add(new Binding<Vector3>(trigger.Position, transform.Position));

			entity.Add("Trigger", new Command
			{
				Action = delegate()
				{
					rift.Enabled.Value = true;
				}
			});

			trigger.Add(new CommandBinding(trigger.PlayerEntered, entity.GetCommand("Trigger")));
		}

		public override void AttachEditorComponents(Entity entity, Main main)
		{
			base.AttachEditorComponents(entity, main);

			Rift.AttachEditorComponents(entity, main, this.Color);
			PlayerTrigger.AttachEditorComponents(entity, main, new Vector3(0.4f, 0.4f, 1.0f));

			VoxelAttachable.AttachEditorComponents(entity, main, entity.Get<Model>().Color);
		}
	}
}
