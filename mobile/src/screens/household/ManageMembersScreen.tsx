import React, { useState } from 'react';
import { Alert, FlatList, StyleSheet, View } from 'react-native';
import {
  ActivityIndicator,
  Avatar,
  Button,
  Chip,
  Dialog,
  Divider,
  List,
  Portal,
  RadioButton,
  Snackbar,
  Text,
  useTheme,
} from 'react-native-paper';
import { useTranslation } from 'react-i18next';
import { useSelector } from 'react-redux';
import type { MemberDto } from '../../store/api/authApi';
import {
  useGetMyHouseholdQuery,
  useRemoveMemberMutation,
  useChangeMemberRoleMutation,
} from '../../store/api/authApi';
import type { RootState } from '../../store';

const ROLES = ['HouseholdAdmin', 'HouseholdMember', 'HouseholdViewer'] as const;
type Role = (typeof ROLES)[number];

export default function ManageMembersScreen() {
  const { t } = useTranslation();
  const { colors } = useTheme();
  const currentUserId = useSelector((state: RootState) => state.auth.user?.id);

  const { data: household, isLoading, isError, refetch } = useGetMyHouseholdQuery();
  const [removeMember, { isLoading: isRemoving }] = useRemoveMemberMutation();
  const [changeMemberRole, { isLoading: isChangingRole }] = useChangeMemberRoleMutation();

  const [roleDialog, setRoleDialog] = useState<{ member: MemberDto; selectedRole: Role } | null>(null);
  const [snackbar, setSnackbar] = useState<string | null>(null);

  function handleRemove(member: MemberDto) {
    if (!household) return;
    Alert.alert(
      t('household.members.removeTitle'),
      t('household.members.removeConfirm', { name: member.displayName }),
      [
        { text: t('common.cancel'), style: 'cancel' },
        {
          text: t('household.members.remove'),
          style: 'destructive',
          onPress: async () => {
            try {
              await removeMember({ householdId: household.id, targetUserId: member.userId }).unwrap();
              setSnackbar(t('household.members.removeSuccess', { name: member.displayName }));
            } catch (err: unknown) {
              const message = (err as { data?: { detail?: string } })?.data?.detail;
              setSnackbar(message ?? t('auth.errors.unknown'));
            }
          },
        },
      ],
    );
  }

  async function handleRoleChange() {
    if (!roleDialog || !household) return;
    try {
      await changeMemberRole({
        householdId: household.id,
        targetUserId: roleDialog.member.userId,
        role: roleDialog.selectedRole,
      }).unwrap();
      setRoleDialog(null);
      setSnackbar(t('household.members.roleChangeSuccess'));
    } catch (err: unknown) {
      const message = (err as { data?: { detail?: string } })?.data?.detail;
      setSnackbar(message ?? t('auth.errors.unknown'));
      setRoleDialog(null);
    }
  }

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  if (isError || !household) {
    return (
      <View style={styles.center}>
        <Text variant="bodyMedium" style={{ color: colors.error }}>{t('auth.errors.unknown')}</Text>
        <Button onPress={refetch} style={styles.retryButton}>{t('common.retry')}</Button>
      </View>
    );
  }

  const isAdmin = household.members.some(
    (m) => m.userId === currentUserId && m.role === 'HouseholdAdmin',
  );

  function roleLabel(role: string): string {
    return t(`household.members.roles.${role}`, { defaultValue: role });
  }

  function roleColor(role: string): string {
    switch (role) {
      case 'HouseholdAdmin': return colors.primary;
      case 'HouseholdViewer': return colors.secondary;
      default: return colors.onSurfaceVariant;
    }
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <FlatList
        data={household.members}
        keyExtractor={(item) => item.userId}
        ItemSeparatorComponent={() => <Divider />}
        renderItem={({ item }) => (
          <List.Item
            title={item.displayName + (item.isCurrentUser ? ` (${t('household.members.you')})` : '')}
            description={() => (
              <Chip
                compact
                textStyle={{ fontSize: 11 }}
                style={[styles.roleChip, { borderColor: roleColor(item.role) }]}
              >
                {roleLabel(item.role)}
              </Chip>
            )}
            left={() =>
              item.avatarUrl
                ? <Avatar.Image size={40} source={{ uri: item.avatarUrl }} style={styles.avatar} />
                : <Avatar.Text size={40} label={item.displayName.charAt(0).toUpperCase()} style={styles.avatar} />
            }
            right={() =>
              isAdmin && !item.isCurrentUser
                ? (
                    <View style={styles.actions}>
                      <List.Icon
                        icon="account-edit"
                        color={colors.primary}
                        onPress={() => setRoleDialog({ member: item, selectedRole: item.role as Role })}
                      />
                      <List.Icon
                        icon="account-remove"
                        color={colors.error}
                        onPress={() => handleRemove(item)}
                      />
                    </View>
                  )
                : null
            }
          />
        )}
      />

      <Portal>
        <Dialog visible={roleDialog !== null} onDismiss={() => setRoleDialog(null)}>
          <Dialog.Title>{t('household.members.changeRoleTitle')}</Dialog.Title>
          <Dialog.Content>
            <RadioButton.Group
              value={roleDialog?.selectedRole ?? ''}
              onValueChange={(v) =>
                roleDialog && setRoleDialog({ ...roleDialog, selectedRole: v as Role })
              }
            >
              {ROLES.map((role) => (
                <RadioButton.Item key={role} label={roleLabel(role)} value={role} />
              ))}
            </RadioButton.Group>
          </Dialog.Content>
          <Dialog.Actions>
            <Button onPress={() => setRoleDialog(null)}>{t('common.cancel')}</Button>
            <Button
              onPress={handleRoleChange}
              loading={isChangingRole}
              disabled={isChangingRole || roleDialog?.selectedRole === roleDialog?.member.role}
            >
              {t('common.ok')}
            </Button>
          </Dialog.Actions>
        </Dialog>
      </Portal>

      <Snackbar
        visible={snackbar !== null}
        onDismiss={() => setSnackbar(null)}
        duration={3000}
      >
        {snackbar}
      </Snackbar>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
  avatar: { marginVertical: 8, marginStart: 8 },
  roleChip: { alignSelf: 'flex-start', marginTop: 4, borderWidth: 1 },
  actions: { flexDirection: 'row', alignItems: 'center' },
  retryButton: { marginTop: 12 },
});
