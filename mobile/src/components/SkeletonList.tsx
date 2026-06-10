import React, { useEffect, useRef } from 'react';
import { Animated, StyleSheet, View } from 'react-native';
import { useTheme } from 'react-native-paper';

interface Props {
  count?: number;
  showThumb?: boolean;
}

export default function SkeletonList({ count = 6, showThumb = true }: Props) {
  const { colors } = useTheme();
  const anim = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    Animated.loop(
      Animated.sequence([
        Animated.timing(anim, { toValue: 1, duration: 650, useNativeDriver: true }),
        Animated.timing(anim, { toValue: 0, duration: 650, useNativeDriver: true }),
      ]),
    ).start();
  }, [anim]);

  const opacity = anim.interpolate({ inputRange: [0, 1], outputRange: [0.4, 0.9] });

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      {Array.from({ length: count }).map((_, i) => (
        <View key={i} style={styles.row}>
          {showThumb && (
            <Animated.View
              style={[
                styles.thumb,
                { backgroundColor: colors.surfaceVariant, opacity },
              ]}
            />
          )}
          <View style={styles.lines}>
            <Animated.View
              style={[
                styles.line1,
                { backgroundColor: colors.surfaceVariant, opacity },
              ]}
            />
            <Animated.View
              style={[
                styles.line2,
                { backgroundColor: colors.surfaceVariant, opacity },
              ]}
            />
          </View>
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  row: { flexDirection: 'row', alignItems: 'center', gap: 14, paddingHorizontal: 16, paddingVertical: 14 },
  thumb: { width: 46, height: 46, borderRadius: 14, flexShrink: 0 },
  lines: { flex: 1, gap: 8 },
  line1: { height: 15, width: '55%', borderRadius: 6 },
  line2: { height: 12, width: '38%', borderRadius: 6 },
});
