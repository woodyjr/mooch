import type { DogActivitySummary, DogDashboard, DogOwnerInvite, DogSummary, PendingDogOwnerInvite } from "./types";

type DogApiResponse = {
  id: string;
  name: string;
  breed?: string | null;
  birthDate?: string | null;
  weightPounds?: number | null;
  bio?: string | null;
  avatarImage?: string | null;
  ownerCount: number;
  isPrimaryOwner: boolean;
};

type DogDashboardApiResponse = {
  dogID: string;
  name: string;
  weeklyGoalMiles: number;
  weeklyMiles: number;
  weeklyAdventures: number;
  weeklyOutsideMinutes: number;
  streakDays: number;
  packRank: number;
  ownerCount: number;
  recentActivities: Array<{
    activityID: string;
    title: string;
    distanceMiles: number;
    durationMinutes: number;
    startedAtUtc: string;
  }>;
  weeklyChallenge: {
    name: string;
    goalMiles: number;
    remainingMiles: number;
  };
};

export type CreateDogInput = {
  name: string;
  breed?: string;
  birthDate?: string;
  weightPounds?: number;
  bio?: string;
  avatarImage?: string;
  avatarFile?: File;
  coOwnerEmail?: string;
};

export type UpdateDogInput = {
  dogId: string;
  name: string;
  breed?: string;
  birthDate?: string;
  weightPounds?: number;
  bio?: string;
};

function toDogSummary(dog: DogApiResponse): DogSummary {
  return {
    id: dog.id,
    name: dog.name,
    breed: dog.breed?.trim() || "Breed not added yet",
    birthDate: dog.birthDate?.trim() || undefined,
    weightPounds: dog.weightPounds ?? undefined,
    bio: dog.bio?.trim() || "No notes yet.",
    avatarImage: dog.avatarImage?.trim() || undefined,
    streakDays: 0,
    ownerCount: dog.ownerCount,
    isPrimaryOwner: dog.isPrimaryOwner
  };
}

function toActivitySummary(activity: DogDashboardApiResponse["recentActivities"][number]): DogActivitySummary {
  return {
    id: activity.activityID,
    title: activity.title,
    distanceMiles: activity.distanceMiles,
    durationMinutes: activity.durationMinutes,
    startedAt: activity.startedAtUtc
  };
}

export async function fetchDogs(): Promise<DogSummary[]> {
  const response = await fetch("/api/dogs", {
    credentials: "include"
  });

  if (!response.ok) {
    throw new Error("Failed to load your dogs.");
  }

  const dogs = (await response.json()) as DogApiResponse[];
  return dogs.map(toDogSummary);
}

export async function fetchDogDashboard(dogId: string): Promise<DogDashboard> {
  const response = await fetch(`/api/dogs/${dogId}/dashboard`, {
    credentials: "include"
  });

  if (!response.ok) {
    throw new Error("Failed to load your dog dashboard.");
  }

  const dashboard = (await response.json()) as DogDashboardApiResponse;
  return {
    dogId: dashboard.dogID,
    name: dashboard.name,
    weeklyGoalMiles: dashboard.weeklyGoalMiles,
    weeklyMiles: dashboard.weeklyMiles,
    weeklyAdventures: dashboard.weeklyAdventures,
    weeklyOutsideMinutes: dashboard.weeklyOutsideMinutes,
    streakDays: dashboard.streakDays,
    packRank: dashboard.packRank,
    ownerCount: dashboard.ownerCount,
    recentActivities: dashboard.recentActivities.map(toActivitySummary),
    weeklyChallenge: dashboard.weeklyChallenge
  };
}

export async function createDog(input: CreateDogInput): Promise<DogSummary> {
  const response = await fetch("/api/dogs", {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      name: input.name,
      breed: input.breed?.trim() || null,
      birthDate: input.birthDate?.trim() || null,
      weightPounds: input.weightPounds ?? null,
      bio: input.bio?.trim() || null,
      avatarImage: input.avatarImage?.trim() || null,
      coOwnerEmail: input.coOwnerEmail?.trim() || null
    })
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Could not save your dog right now.");
  }

  const dog = (await response.json()) as DogApiResponse;
  const createdDog = toDogSummary(dog);

  if (input.avatarFile) {
    return uploadDogAvatar(createdDog.id, input.avatarFile);
  }

  return createdDog;
}

export async function updateDog(input: UpdateDogInput): Promise<DogSummary> {
  const response = await fetch(`/api/dogs/${input.dogId}`, {
    method: "PUT",
    credentials: "include",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      name: input.name,
      breed: input.breed?.trim() || null,
      birthDate: input.birthDate?.trim() || null,
      weightPounds: input.weightPounds ?? null,
      bio: input.bio?.trim() || null
    })
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Could not update that dog profile.");
  }

  const dog = (await response.json()) as DogApiResponse;
  return toDogSummary(dog);
}

export async function uploadDogAvatar(dogId: string, file: File): Promise<DogSummary> {
  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch(`/api/dogs/${dogId}/avatar`, {
    method: "POST",
    credentials: "include",
    body: formData
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Could not upload that dog photo.");
  }

  const dog = (await response.json()) as DogApiResponse;
  return toDogSummary(dog);
}

export async function deleteDog(dogId: string): Promise<void> {
  const response = await fetch(`/api/dogs/${dogId}`, {
    method: "DELETE",
    credentials: "include"
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Could not remove that dog from your pack.");
  }
}

export async function fetchDogOwnerInvites(dogId: string): Promise<DogOwnerInvite[]> {
  const response = await fetch(`/api/dogs/${dogId}/owner-invites`, {
    credentials: "include"
  });

  if (!response.ok) {
    throw new Error("Failed to load owner invites.");
  }

  return response.json() as Promise<DogOwnerInvite[]>;
}

export async function inviteDogOwner(dogId: string, email: string): Promise<DogOwnerInvite> {
  const response = await fetch(`/api/dogs/${dogId}/owner-invites`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      email: email.trim()
    })
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Could not send that co-owner invite.");
  }

  return response.json() as Promise<DogOwnerInvite>;
}

export async function fetchPendingDogOwnerInvites(): Promise<PendingDogOwnerInvite[]> {
  const response = await fetch("/api/dogs/owner-invites/pending", {
    credentials: "include"
  });

  if (!response.ok) {
    throw new Error("Failed to load pending dog invites.");
  }

  const invites = (await response.json()) as Array<{
    id: string;
    dogID: string;
    dogName: string;
    invitedByDisplayName: string;
    createdDateUtc: string;
  }>;

  return invites.map((invite) => ({
    id: invite.id,
    dogId: invite.dogID,
    dogName: invite.dogName,
    invitedByDisplayName: invite.invitedByDisplayName,
    createdDateUtc: invite.createdDateUtc
  }));
}

export async function acceptDogOwnerInvite(inviteId: string): Promise<DogOwnerInvite> {
  const response = await fetch(`/api/dogs/owner-invites/${inviteId}/accept`, {
    method: "POST",
    credentials: "include"
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Could not accept that dog invite.");
  }

  return response.json() as Promise<DogOwnerInvite>;
}

export async function declineDogOwnerInvite(inviteId: string): Promise<DogOwnerInvite> {
  const response = await fetch(`/api/dogs/owner-invites/${inviteId}/decline`, {
    method: "POST",
    credentials: "include"
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Could not decline that dog invite.");
  }

  return response.json() as Promise<DogOwnerInvite>;
}
