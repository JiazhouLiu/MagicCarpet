using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using System;
using Unity.Mathematics;

public class SMEntity : MonoBehaviour
{
    [SerializeField] private TextAsset DataSource;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;


    private List<Housing> PropertyCollection;

    private readonly char lineSeperater = '\n'; // It defines line seperate character
    private readonly char fieldSeperator = ','; // It defines field seperate chracter
    
    private void Start()
    {
        PropertyCollection = new List<Housing>();

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(PropertyComponent),
            typeof(MoveSpeedComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(CompositeScale)
        );

        ReadData(DataSource);

        NativeArray<Entity> entityArray = new NativeArray<Entity>(PropertyCollection.Count, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        for (int i = 0; i < PropertyCollection.Count; i++) {
            Entity entity = entityArray[i];
            Housing property = PropertyCollection[i];
            entityManager.SetComponentData(entity,
                new PropertyComponent {
                    ID = property.ID,
                    Suburb = property.Suburb,
                    PropertyType = property.Type,
                    Price = property.Price,
                    Result = property.Result,
                    Seller = property.Seller,
                    //Date = property.Date,
                    Bedroom = property.Bedroom,
                    Bathroom = property.Bathroom,
                    Car = property.Car,
                    Landsize = property.Landsize,
                    YearBuilt = property.YearBuilt,
                    CouncilArea = property.CouncilArea,
                    Latitude = property.Latitude,
                    Longtitude = property.Longtitude,
                    RegionName = property.RegionName,

                    x = UnityEngine.Random.Range(-5f, 5f),
                    y = UnityEngine.Random.Range(-5f, 5f),
                    z = UnityEngine.Random.Range(-5f, 5f),

                    //speed = MovingSpeed
                }
            );

            entityManager.SetComponentData(entity, 
                new Translation {
                    Value = new float3(UnityEngine.Random.Range(-5, 5f), 
                    UnityEngine.Random.Range(-5, 5f), UnityEngine.Random.Range(-5, 5f))
                }
            );

            entityManager.SetComponentData(entity,
                new MoveSpeedComponent
                {
                    speed = UnityEngine.Random.Range(0, 0.01f)
                }
            );

            entityManager.SetSharedComponentData(entity, 
                new RenderMesh {
                    mesh = mesh,
                    material = material
                }
            );

            entityManager.SetComponentData(entity, 
                new CompositeScale{
                    Value = float4x4.Scale(0.1f, 0.1f, 0.1f)
                }
            );
        }

        entityArray.Dispose();
    }

    private void ReadData(TextAsset ta)
    {
        string[] lines = ta.text.Split(lineSeperater);
        int dataLength = lines.Length;

        for (int i = 1; i < dataLength; i++)
        {
            if (lines[i].Length > 10)
            {
                Housing property = new Housing(i, lines[i].Split(fieldSeperator)[0], lines[i].Split(fieldSeperator)[1],
                int.Parse(lines[i].Split(fieldSeperator)[2]), lines[i].Split(fieldSeperator)[3],
                lines[i].Split(fieldSeperator)[4], DateTime.Parse(lines[i].Split(fieldSeperator)[5]),
                int.Parse(lines[i].Split(fieldSeperator)[6]), int.Parse(lines[i].Split(fieldSeperator)[7]),
                int.Parse(lines[i].Split(fieldSeperator)[8]), int.Parse(lines[i].Split(fieldSeperator)[9]),
                int.Parse(lines[i].Split(fieldSeperator)[10]), lines[i].Split(fieldSeperator)[11], float.Parse(lines[i].Split(fieldSeperator)[12]),
                float.Parse(lines[i].Split(fieldSeperator)[13]), lines[i].Split(fieldSeperator)[14]);

                PropertyCollection.Add(property);
            }
        }
    }

}
