using System.Numerics;
using System.Text.Json;
using PallasDotnet.Models;

namespace PallasDotnet;

public class Utils
{
    public static Point MapPallasPoint(PallasDotnetRs.PallasDotnetRs.Point rsPoint)
        => new(rsPoint.slot, new Hash([.. rsPoint.hash]));

    public static Block MapPallasBlock(PallasDotnetRs.PallasDotnetRs.Block rsBlock)
        => new(
            rsBlock.slot,
            new Hash([.. rsBlock.hash]),
            rsBlock.number,
            rsBlock.transactionBodies.Select(MapPallasTransactionBody)
        );

    public static TransactionBody MapPallasTransactionBody(PallasDotnetRs.PallasDotnetRs.TransactionBody rsTransactionBody)
        => new(
            new Hash([.. rsTransactionBody.id]),
            rsTransactionBody.index,
            rsTransactionBody.era,
            rsTransactionBody.inputs.Select(MapPallasTransactionInput),
            rsTransactionBody.outputs.Select(MapPallasTransactionOutput),
            MapPallasMultiAsset(rsTransactionBody.mint),
            rsTransactionBody.metadata != string.Empty ? JsonSerializer.Deserialize<JsonElement>(rsTransactionBody.metadata) : null,
            rsTransactionBody.redeemers?.Select(r => new Redeemer(
                (RedeemerTag)r.tag,
                r.index,
                [.. r.data],
                new ExUnits(r.exUnits.mem, r.exUnits.steps)
            )),
            [.. rsTransactionBody.raw]
        );

    public static TransactionInput MapPallasTransactionInput(PallasDotnetRs.PallasDotnetRs.TransactionInput rsTransactionInput)
        => new(
            new Hash([.. rsTransactionInput.id]),
            rsTransactionInput.index
        );

    public static TransactionOutput MapPallasTransactionOutput(PallasDotnetRs.PallasDotnetRs.TransactionOutput rsTransactionOutput)
        => new(
            new Address([.. rsTransactionOutput.address]),
            MapPallasValue(rsTransactionOutput.amount),
            rsTransactionOutput.index,
            rsTransactionOutput.datum as object is null
                ? null
                : (DatumType)rsTransactionOutput.datum.datumType == 0 ? null 
                    : new Datum(
                        (DatumType)rsTransactionOutput.datum.datumType,
                        [.. rsTransactionOutput.datum.data]
                    ),
            [.. rsTransactionOutput.raw]
        );

    public static Value MapPallasValue(PallasDotnetRs.PallasDotnetRs.Value rsValue)
        => new(
            rsValue.coin,
            MapPallasMultiAsset(rsValue.multiAsset)
        );

    public static Dictionary<Hash, Dictionary<Hash, T>> MapPallasMultiAsset<T>(Dictionary<List<byte>, Dictionary<List<byte>, T>> rsMultiAsset) where T : INumber<T>
        => rsMultiAsset.ToDictionary(
            k => new Hash([.. k.Key]),
            v => v.Value.ToDictionary(
                k => new Hash([.. k.Key]),
                v => v.Value
            )
        );
}