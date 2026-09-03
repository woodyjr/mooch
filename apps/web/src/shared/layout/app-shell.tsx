import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { NavLink, Outlet, useNavigate } from "react-router-dom";

import { useAuth } from "../../features/auth/auth-context";
import { acceptDogOwnerInvite, declineDogOwnerInvite, fetchDogs, fetchPendingDogOwnerInvites } from "../../features/dogs/api";
import type { DogSummary } from "../../features/dogs/types";
import { Button } from "../ui/button";
import { MoochIcon } from "../ui/mooch-icon";

const desktopNavigation = [
  { to: "/app", label: "Dashboard", icon: "grid" as const },
  { to: "/app/walk", label: "Activities", icon: "route" as const },
  { to: "/app/ranks", label: "Leaderboard", icon: "trophy" as const },
  { to: "/app/dogs", label: "Dogs", icon: "paw" as const }
];

const mobileNavigation = [
  { to: "/app", label: "Home", icon: "home" as const },
  { to: "/app/walk", label: "Activity", icon: "route" as const },
  { to: "/app/pack", label: "Pack", icon: "pack" as const },
  { to: "/app/ranks", label: "Ranks", icon: "ranks" as const },
  { to: "/app/dogs", label: "Dogs", icon: "paw" as const }
];

function getInitials(name: string | undefined): string {
  if (!name) {
    return "M";
  }

  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
}

function getSidebarDog(dogs: DogSummary[]) {
  return dogs.find((dog) => dog.name.toLowerCase() === "mooch") ?? dogs[0];
}

export function AppShell() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user, logout } = useAuth();
  const { data: pendingInvites = [] } = useQuery({
    queryKey: ["pending-dog-owner-invites"],
    queryFn: fetchPendingDogOwnerInvites
  });
  const { data: dogs = [] } = useQuery({
    queryKey: ["dogs"],
    queryFn: fetchDogs
  });

  const acceptInviteMutation = useMutation({
    mutationFn: acceptDogOwnerInvite,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["pending-dog-owner-invites"] });
      await queryClient.invalidateQueries({ queryKey: ["dogs"] });
      await queryClient.invalidateQueries({ queryKey: ["dog-dashboard"] });
    }
  });

  const declineInviteMutation = useMutation({
    mutationFn: declineDogOwnerInvite,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["pending-dog-owner-invites"] });
    }
  });

  async function handleLogout() {
    await logout();
    navigate("/");
  }

  const userFirstName = user?.displayName?.split(" ", 2)[0] ?? "Josh";
  const sidebarDog = getSidebarDog(dogs);

  return (
    <div className="app-shell app-shell--desktop">
      <aside className="app-sidebar">
        <div className="app-sidebar__top">
          <NavLink className="app-sidebar__brand" to="/app">
            <span className="app-sidebar__brand-copy">Mooch</span>
            <span className="app-sidebar__brand-mark">
              <MoochIcon name="paw" />
            </span>
          </NavLink>

          <div className="app-sidebar__dog-card">
            <span className="app-sidebar__dog-avatar">
              {sidebarDog?.avatarImage ? (
                <img alt={sidebarDog.name} src={sidebarDog.avatarImage} />
              ) : (
                <span>{getInitials(sidebarDog?.name ?? userFirstName)}</span>
              )}
            </span>
            <strong>{sidebarDog?.name ?? "Mooch"}</strong>
            <span className="app-sidebar__dog-badge">
              <MoochIcon name="paw" />
              Good boy
            </span>
          </div>

          <nav className="app-sidebar__nav">
            {desktopNavigation.map((item) => (
              <NavLink
                className={({ isActive }) =>
                  ["app-sidebar__link", isActive ? "app-sidebar__link--active" : ""]
                    .filter(Boolean)
                    .join(" ")
                }
                end={item.to === "/app"}
                key={item.to}
                to={item.to}
              >
                <span className="app-sidebar__link-icon">
                  <MoochIcon name={item.icon} />
                </span>
                <span>{item.label}</span>
              </NavLink>
            ))}
          </nav>

          <button className="app-sidebar__utility" type="button">
            <span className="app-sidebar__link-icon">
              <MoochIcon name="user" />
            </span>
            <span>Profile</span>
          </button>

          <button className="app-sidebar__utility" type="button">
            <span className="app-sidebar__link-icon">
              <MoochIcon name="settings" />
            </span>
            <span>Settings</span>
          </button>
        </div>

        <div className="app-sidebar__bottom">
          <button className="app-sidebar__utility" onClick={() => void handleLogout()} type="button">
            <span className="app-sidebar__link-icon">
              <MoochIcon name="logout" />
            </span>
            <span>Log out</span>
          </button>
        </div>
      </aside>

      <div className="app-shell__main">
        <header className="app-shell__mobile-header">
          <NavLink className="app-shell__mobile-brand" to="/app">
            <span className="app-shell__mobile-brand-mark">
              <MoochIcon name="paw" />
            </span>
            <span>Mooch</span>
          </NavLink>
          <Button aria-label="Notifications" type="button" variant="icon">
            <MoochIcon name="bell" />
          </Button>
        </header>

        <main className="app-shell__workspace">
          {pendingInvites.length > 0 && (
            <section className="invite-banner">
              <div className="invite-banner__header">
                <div>
                  <p className="eyebrow">Dog invite</p>
                  <h2>You have {pendingInvites.length} dog invite{pendingInvites.length === 1 ? "" : "s"} waiting</h2>
                </div>
                <NavLink className="section-link" to="/app/dogs">
                  View dogs
                </NavLink>
              </div>

              <div className="invite-banner__list">
                {pendingInvites.map((invite) => (
                  <article className="invite-banner__item" key={invite.id}>
                    <div>
                      <h3>Join {invite.dogName}</h3>
                      <p>{invite.invitedByDisplayName} invited you to the shared dog profile on {new Date(invite.createdDateUtc).toLocaleDateString("en-US", { month: "long", day: "numeric" })}.</p>
                    </div>
                    <div className="invite-banner__actions">
                      <Button
                        disabled={declineInviteMutation.isPending}
                        onClick={() => void declineInviteMutation.mutateAsync(invite.id)}
                        type="button"
                        variant="secondary"
                      >
                        Not now
                      </Button>
                      <Button
                        disabled={acceptInviteMutation.isPending}
                        onClick={() => void acceptInviteMutation.mutateAsync(invite.id)}
                        type="button"
                      >
                        Join dog
                      </Button>
                    </div>
                  </article>
                ))}
              </div>
            </section>
          )}

          <Outlet />
        </main>
      </div>

      <nav className="bottom-nav">
        {mobileNavigation.map((item) => (
          <NavLink
            className={({ isActive }) => ["bottom-nav__link", isActive ? "bottom-nav__link--active" : ""].filter(Boolean).join(" ")}
            end={item.to === "/app"}
            key={item.to}
            to={item.to}
          >
            <span className="bottom-nav__icon">
              <MoochIcon name={item.icon} />
            </span>
            <span className="bottom-nav__label">{item.label}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
