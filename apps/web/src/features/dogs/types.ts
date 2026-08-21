export type DogSummary = {
  id: string;
  name: string;
  breed: string;
  bio: string;
  avatarImage?: string;
  streakDays: number;
  ownerCount: number;
  isPrimaryOwner: boolean;
};

export type DogOwnerInvite = {
  id: string;
  email: string;
  status: string;
  createdDateUtc: string;
};

export type PendingDogOwnerInvite = {
  id: string;
  dogId: string;
  dogName: string;
  invitedByDisplayName: string;
  createdDateUtc: string;
};

export type DogActivitySummary = {
  id: string;
  title: string;
  distanceMiles: number;
  durationMinutes: number;
  startedAt: string;
};

export type DogDashboard = {
  dogId: string;
  name: string;
  weeklyGoalMiles: number;
  weeklyMiles: number;
  weeklyAdventures: number;
  weeklyOutsideMinutes: number;
  streakDays: number;
  packRank: number;
  ownerCount: number;
  recentActivities: DogActivitySummary[];
  weeklyChallenge: {
    name: string;
    goalMiles: number;
    remainingMiles: number;
  };
};
