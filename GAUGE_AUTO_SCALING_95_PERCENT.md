# Gauge Auto-Scaling Behavior

## Overview

The disk and network gauges automatically adjust their maximum scale based on current and historical throughput values to ensure optimal readability across different usage levels.

## Scaling Threshold: 95%

### When Scale Changes:
- **Scale Up**: When current or peak value exceeds **95%** of the current scale
- **Scale Down**: Never automatically scales down (uses peak tracking)

### Example:
```
Current Scale: 100 Mbps
Threshold: 95 Mbps (95% of 100)

If throughput reaches 96 Mbps:
? Scale automatically increases to 250 Mbps

New Threshold: 237.5 Mbps (95% of 250)
```

## Available Scales

The system uses the following predefined scales (in Mbps):

| Scale | Description | Typical Use Case |
|-------|-------------|------------------|
| 10 Mbps | Minimum scale | Very light activity |
| 25 Mbps | Low scale | Light browsing, email |
| 50 Mbps | Low-medium | Standard browsing |
| 100 Mbps | Medium | **Default starting scale** |
| 250 Mbps | Medium-high | HD streaming, downloads |
| 500 Mbps | High | Multiple streams, large transfers |
| 1000 Mbps | Very high | 1 Gbps network (Gigabit) |
| 2500 Mbps | Extreme | 2.5 Gbps network |
| 5000 Mbps | Maximum | 5 Gbps network |
| 10000 Mbps | Absolute max | 10 Gbps network |

## Scale Selection Logic

### Algorithm:
```csharp
float GetAutoScale(float currentValue, float peakValue)
{
    float targetValue = Math.Max(currentValue, peakValue);
    
    foreach (float scale in SCALE_OPTIONS)
    {
        if (targetValue <= scale * 0.95f)  // 95% threshold
        {
            return scale;
        }
    }
    
    return SCALE_OPTIONS[^1]; // Return maximum if all exceeded
}
```

### Key Points:
1. **Uses Peak Tracking**: Considers both current value and historical peak
2. **Prevents Flicker**: Once scaled up, won't scale back down until application restart
3. **Smooth Transitions**: 95% threshold prevents constant scale changes
4. **Headroom**: Always leaves 5% headroom above current usage

## Practical Examples

### Scenario 1: Gradual Increase
```
Time  | Speed    | Current Scale | Action
------|----------|---------------|------------------
0s    | 15 Mbps  | 100 Mbps      | Default scale
10s   | 45 Mbps  | 100 Mbps      | Within threshold
20s   | 87 Mbps  | 100 Mbps      | Within threshold
30s   | 96 Mbps  | 250 Mbps      | Scaled up (>95 Mbps)
40s   | 124 Mbps | 250 Mbps      | Within threshold
50s   | 239 Mbps | 500 Mbps      | Scaled up (>237.5 Mbps)
```

### Scenario 2: Burst Activity
```
Time  | Speed    | Current Scale | Peak    | Action
------|----------|---------------|---------|------------------
0s    | 5 Mbps   | 100 Mbps      | 5 Mbps  | Default
5s    | 450 Mbps | 500 Mbps      | 450 Mbps| Scaled up immediately
10s   | 20 Mbps  | 500 Mbps      | 450 Mbps| Scale stays at 500
15s   | 50 Mbps  | 500 Mbps      | 450 Mbps| Scale stays at 500
```

### Scenario 3: Network Capability Test
```
Initial: 100 Mbps scale
Download at 980 Mbps:
? Scales to 1000 Mbps (1 Gbps scale)

Needle shows at 98% of gauge
```

## Visual Feedback

### Gauge Display:
```
Disk Speed
246.3 Mbps (Scale: 500)
     ?           ?
  Current    Current Max
   Speed       Scale
```

### Needle Position Interpretation:
- **0-70%**: Normal operating range
- **70-90%**: Moderate to high activity
- **90-95%**: Near threshold (about to scale up)
- **95-100%**: Scale will increase next update

## Benefits of 95% Threshold

### Advantages:
1. **Maximum Visibility**: Needle visible for most of gauge range
2. **Minimal Scale Changes**: Only scales when truly necessary
3. **Clear Reading**: Values don't bunch at the top
4. **Smooth Operation**: Avoids constant re-scaling

### vs. Lower Thresholds:
- **80% Threshold**: More frequent scale changes, more headroom but harder to read small values
- **90% Threshold**: Balanced but still changes often
- **95% Threshold**: ? Optimal - rarely scales unless sustained high usage
- **100% Threshold**: Would peg needle at max before scaling (bad UX)

## Scale Persistence

### Session Behavior:
```
Application Start:
- Disk: 100 Mbps (default)
- Network: 100 Mbps (default)

During Session:
- Peak: 450 Mbps reached
- Scale: Increased to 500 Mbps
- Remains: At 500 Mbps for entire session

After Restart:
- Reset: Back to 100 Mbps default
- Relearn: Will scale up as needed again
```

### Why Not Save Scale?
- **Activity varies**: Different usage patterns each session
- **Cleaner start**: Fresh baseline each time
- **No confusion**: Always starts at predictable state

## Technical Implementation

### Update Flow:
```
1. Monitor measures throughput
   ?
2. Update peak value: Math.Max(peak, current)
   ?
3. Calculate required scale: GetAutoScale(current, peak)
   ?
4. Update gauge: diskUsageGauge.MaxValue = newScale
   ?
5. Display: "246.3 Mbps (Scale: 500)"
```

### Thread Safety:
- Peak values tracked per gauge
- Updated on UI thread during timer tick
- No concurrent modification issues

## User Experience

### What Users See:
1. **Starting**: Gauges begin at 100 Mbps scale
2. **Normal Use**: Needle moves naturally within current scale
3. **High Activity**: When exceeding 95%, scale automatically increases
4. **Scale Display**: Always shown in label below gauge
5. **Visual Continuity**: Needle position adjusts smoothly to new scale

### Expected Behavior:
```
Light Use (5-50 Mbps):
? Stays at 100 Mbps scale
? Needle in lower range

Medium Use (50-200 Mbps):
? Scales to 250 Mbps
? Needle in mid-range

Heavy Use (200-900 Mbps):
? Scales to 1000 Mbps
? Needle readable throughout
```

## Debugging Scale Changes

### Log Messages:
```csharp
Debug.WriteLine($"Disk: {totalMbps:F1} Mbps, Scale: {newScale:F0} Mbps, Peak: {_diskPeakMbps:F1} Mbps");
```

### Monitor Values:
- `_diskPeakMbps`: Current session peak for disk
- `_networkPeakMbps`: Current session peak for network
- `diskUsageGauge.MaxValue`: Current scale setting
- `totalMbps`: Current measured throughput

## Configuration

### Changing Threshold:
To modify the 95% threshold, edit the `GetAutoScale` method:

```csharp
if (targetValue <= scale * 0.95f)  // Change 0.95 to desired value
```

### Common Thresholds:
- `0.90f`: 90% - More frequent scaling
- `0.95f`: 95% - **Current setting** (recommended)
- `0.98f`: 98% - Less frequent scaling
- `1.00f`: 100% - Only scale when pegged at max

### Adding New Scales:
Edit the `SCALE_OPTIONS` array:
```csharp
private static readonly float[] SCALE_OPTIONS = { 
    10f, 25f, 50f, 100f, 250f, 500f, 1000f, 2500f, 5000f, 10000f,
    // Add new scales here, in ascending order
    20000f  // Example: 20 Gbps
};
```

## Summary

The **95% threshold** provides optimal balance between:
- ? Readable gauge display
- ? Stable scale selection
- ? Appropriate headroom
- ? Minimal scale changes
- ? Clear visual feedback

Users can always see the current speed and scale at a glance, with automatic adjustments ensuring the gauge remains easy to read at any throughput level.
