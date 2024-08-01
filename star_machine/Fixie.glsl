
// Don't.
#define FORCE_ENABLE_INT64_HACK 0


#if FORCE_ENABLE_INT64_HACK

/* ------------------------------------------------------------------------- *\
 *    What's a validation error and random broken builds between friends?    *
\* ------------------------------------------------------------------------- */

#extension GL_ARB_gpu_shader_int64 : require


struct Fixie
{
    i64vec3 V64;
};


Fixie Unpack(uvec3 L, uvec3 H)
{
    Fixie Result;
    for (int Channel = 0; Channel < 3; ++Channel)
    {
        Result.V64[Channel] = int64_t(uint64_t(L[Channel]) | (uint64_t(H[Channel]) << 32));
    }
    return Result;
}


Fixie Sub(Fixie LHS, Fixie RHS)
{
    Fixie Result;
    Result.V64 = LHS.V64 - RHS.V64;
    return Result;
}


Fixie Abs(Fixie V, out ivec3 Signs)
{
    Signs = ivec3(sign(V.V64));
    V.V64 = abs(V.V64);
    return V;
}


vec3 FixedPointToFloat(Fixie V)
{
    const int UnitOffset = 16;
    const int64_t UnitValue = 1 << UnitOffset;
    const int64_t DecimalMask = UnitValue - 1;
    const float UnitReciprocal = 1.0 / float(UnitValue);

    vec3 Result;
    for (int Channel = 0; Channel < 3; ++Channel)
    {
        float Whole = float(V.V64[Channel] >> UnitOffset);
        float Fract = float(V.V64[Channel] & DecimalMask) * UnitReciprocal;
        Result[Channel] = Whole + Fract;
    }
    return Result;
}


#else

/* ------------------------------------------------------------------------- *\
 *          Emulate i64vec3 vectors with a pair of uvec3 vectors.            *
\* ------------------------------------------------------------------------- */


struct Fixie
{
    uvec3 L;
    uvec3 H;
};


Fixie Unpack(uvec3 L, uvec3 H)
{
    Fixie V;
    V.L = L;
    V.H = H;
    return V;
}


Fixie Sub(Fixie LHS, Fixie RHS)
{
    Fixie Result;
    uvec3 Borrow;
    Result.L = usubBorrow(LHS.L, RHS.L, Borrow);
    Result.H = LHS.H - RHS.H - Borrow;
    return Result;
}


Fixie Abs(Fixie V, out ivec3 Signs)
{
    uvec3 Minus = (V.H >> 31u) & 1u;
    uvec3 Mask = Minus * 0xFFFFFFFFu;
    uvec3 Borrow;
    V.L = usubBorrow(V.L, Minus, Borrow) ^ Mask;
    V.H = (V.H - Borrow) ^ Mask;
    Signs = ivec3(1) - ivec3(Minus * 2u);
    return V;
}


vec3 FixedPointToFloat(Fixie V)
{
    ivec3 Signs;
    V = Abs(V, Signs);

    const int UnitOffset = 16;
    const int WordOffset = 32;
    const float ScaleL = 1.0 / float(1 << UnitOffset);
    const float ScaleH = float(1 << (WordOffset - UnitOffset));
    return (vec3(V.L) * ScaleL + vec3(V.H) * ScaleH) * Signs;
}


#endif
