// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Issue https://github.com/dotnet/coreclr/issues/2580 fix description
//
// The changes fix a bug generating 16-bit signed comparison code to                                 
// compare unsigned 16-bit values.                                                                   
//                        
// int c = GT(ushort a, ushort b)                                                                    
//
// Give the comparison above, RyuJIT generates 16-bit cmp instruction and its signed-ness should be properly handled.                                                                                    
//                        
// Code generated by RyuJIT:                                                                         
//                        
// movzx    rcx, word  ptr [rdi+8] ; load a                                                          
// movzx    rax, word  ptr [rsi+8] ; load b                                                          
// cmp      cx, ax                   ; compare only 16 bits                                          
// setg     cl                       ; set if greater (signed comparison) <- signed comparison for unsigned type                           
//                        
// After fix:                                                                                        
//                         
// movzx    rcx, word  ptr [rdi+8] ; load a                                                          
// movzx    rax, word  ptr [rsi+8] ; load b                                                          
// cmp      cx, ax                 ; compare only 16 bits                                            
// seta     cl                     ; set if above (unsigned comparison)  <-- Fixed instruction       

internal static class MicrsoftNETBug
{
    private struct S
    {
        public readonly ushort D;

        public S(ushort d) { D = d; }
        public static bool operator >(S a, S b) { return a.D > b.D; }
        public static bool operator <(S a, S b) { return a.D < b.D; }
    }


    private class C
    {
        public S D;

        public C(ushort d) { D = new S(d); }
    }



    private static int Main()
    {
        System.Console.WriteLine(".NET 4.6.01055 bug repro: unshort comparison below incorrectly outputs False (only) when code is optimized");

        C c3 = new C(0);
        C c1 = new C(256 * 128 - 1); // 0x7fff
        C c2 = new C(256 * 128);     // 0x8000

        // The condition below with values is 0x7fff > 0 && 0x8000 > 0, and
        // it must be true for ushort type.
        if (c1.D > c3.D && c2.D > c3.D)
        {
            System.Console.WriteLine("PASS");
            return 100;
        }
        else
        {
            System.Console.WriteLine("FAIL");
            return 101;
        }
    }
}