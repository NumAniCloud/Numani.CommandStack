﻿using System;
using System.Threading.Tasks;
using Numani.CommandStack.Maybe;

namespace Numani.CommandStack.Pipes2;

public sealed class Step2<TSource, TMap, TFinal> : ICommandPipe2<TSource, TFinal>
{
    public required Func<TSource, Task<IMaybe<TMap>>> Function { get; init; }
    public required ICommandPipe2<TMap, TFinal> Rest { get; init; }
    
    public async Task<IMaybe<TFinal>> RunAsync(TSource source)
    {
    BackStep:
        var step = await Function.Invoke(source);
        if (step is not Just<TMap> stepJust)
        {
            return Maybe.Maybe.Nothing<TFinal>();
        }

        var final = await Rest.RunAsync(stepJust.Value);
        if (final is not Just<TFinal> finalJust)
        {
            goto BackStep;
        }

        return finalJust;
    }

    public ICommandPipe2<TSource, TNewFinal> WithTail<TNewFinal>(ICommandPipe2<TFinal, TNewFinal> tail)
    {
        return new Step2<TSource, TMap, TNewFinal>()
        {
            Function = Function,
            Rest = Rest.WithTail(tail)
        };
    }
}