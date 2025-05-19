using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessingStack
{
    // CommandBuffer postBuffer = new CommandBuffer {
    //     name = "Post Processing"
    // };

    // ScriptableRenderContext context;

    public void RenderPostProcessing(CommandBuffer postBuffer, ScriptableRenderContext context)
    {
        postBuffer.Blit(sourceId, BuiltinRenderTextureType.CameraTarget);
        context.ExecuteCommandBuffer(postBuffer);
        postBuffer.Clear();
    }
}
