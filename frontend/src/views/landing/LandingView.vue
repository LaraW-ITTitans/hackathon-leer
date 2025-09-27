<template>
  <el-container class="page">
    <el-header class="header" height="72px">
      <div class="brand" @click="scrollTo('top')" role="button" aria-label="Zur Startseite">
        <el-icon class="logo"><Cpu /></el-icon>
        <strong>Datenraum Ostfriesland</strong>
      </div>
      <nav class="nav">
        <a href="#features" @click.prevent="scrollTo('features')">Funktionen</a>
        <a href="#how" @click.prevent="scrollTo('how')">So funktioniert's</a>
        <a href="#faq" @click.prevent="scrollTo('faq')">FAQ</a>
        <el-button type="primary" round @click="toLogin()">Jetzt mitmachen</el-button>
      </nav>
    </el-header>

    <el-main class="main" id="top">
      <!-- HERO -->
      <section class="hero" :style="heroGradient">
        <div class="hero__content">
          <div class="hero__eyebrow">
            <el-tag effect="dark" round type="success">Beta</el-tag>
            <span>Offener, sicherer und nutzerzentrierter Datenraum</span>
          </div>
          <h1>Transparente Daten. Smarte Zugänge. Mehr Wert für Ostfriesland.</h1>
          <p class="lede">
            Der <strong>Daten&nbsp;Raum&nbsp;Leer</strong> ist der zentrale Katalog für Datenquellen aus Verwaltung,
            Wirtschaft, Wissenschaft und Zivilgesellschaft – mit Marktplatz für Bedarfe und
            zertifikatsbasiertem Zugriffsmanagement.
          </p>
          <div class="hero__cta">
            <el-button size="large" type="primary" round @click="scrollTo('features')">
              <el-icon><Compass /></el-icon>
              Entdecken
            </el-button>
            <el-button size="large" round @click="scrollTo('cta')">
              <el-icon><DocumentAdd /></el-icon>
              Bedarf melden
            </el-button>
          </div>
          <ul class="hero__highlights">
            <li><el-icon><Search /></el-icon> Datenkatalog mit Suche & Filtern</li>
            <li><el-icon><CollectionTag /></el-icon> Bedarfsmeldungen & Matching</li>
            <li><el-icon><Lock /></el-icon> Zertifikats- & Rollenmanagement</li>
          </ul>
        </div>
      </section>

      <!-- FEATURES -->
      <section class="section" id="features">
        <h2>Was bietet der Datenraum?</h2>
        <el-row :gutter="16">
          <el-col v-for="f in features" :key="f.title" :xs="24" :sm="12" :md="12" :lg="6">
            <el-card shadow="hover" class="feature">
              <div class="feature__icon">
                <component :is="f.icon" />
              </div>
              <h3>{{ f.title }}</h3>
              <p>{{ f.desc }}</p>
              <div class="feature__tags">
                <el-tag v-for="t in f.tags" :key="t" round size="small">{{ t }}</el-tag>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </section>

      <!-- HOW IT WORKS -->
      <section class="section" id="how">
        <h2>So funktioniert's</h2>
        <el-timeline>
          <el-timeline-item v-for="(s, i) in steps" :key="i" :timestamp="s.kicker" placement="none">
            <el-card class="step" shadow="never">
              <div class="step__icon">
                <component :is="s.icon" />
              </div>
              <div>
                <h3>{{ s.title }}</h3>
                <p>{{ s.text }}</p>
              </div>
            </el-card>
          </el-timeline-item>
        </el-timeline>
      </section>

      <!-- STATS / TRUST -->
      <section class="section stats">
        <el-row :gutter="16">
          <el-col :xs="12" :sm="6" v-for="stat in stats" :key="stat.label">
            <div class="stat">
              <div class="stat__value">{{ stat.value }}</div>
              <div class="stat__label">{{ stat.label }}</div>
            </div>
          </el-col>
        </el-row>
      </section>

      <!-- CTA / NEWSLETTER / CONTACT -->
      <section class="section cta" id="cta">
        <el-row :gutter="16" align="middle">
          <el-col :xs="24" :md="14">
            <h2>Mach mit und gestalte den Datenraum</h2>
            <p>
              Du willst Daten teilen, Bedarfe veröffentlichen oder Pilotpartner werden? Trag dich ein und wir melden uns mit den nächsten Schritten.
            </p>
            <div class="newsletter">
              <el-input
                v-model="email"
                size="large"
                placeholder="E-Mail-Adresse"
                @keyup.enter="subscribe"
                aria-label="E-Mail für Updates"
              >
                <template #prefix>
                  <el-icon><Message /></el-icon>
                </template>
                <template #append>
                  <el-button type="primary" @click="subscribe">Updates erhalten</el-button>
                </template>
              </el-input>
              <small class="hint">Wir senden nur relevante Infos. Abmeldung jederzeit.</small>
            </div>
          </el-col>
          <el-col :xs="24" :md="10">
            <el-card class="contact" shadow="hover">
              <h3>Kontakt</h3>
              <p>Koordination – Landkreis Leer</p>
              <ul class="contact__list">
                <li><el-icon><OfficeBuilding /></el-icon> Rathausstraße 1, 26789 Leer (Ostfriesland)</li>
                <li><el-icon><Phone /></el-icon> 0491 / 123-456</li>
                <li><el-icon><Message /></el-icon> datenraum@lk-leer.de</li>
              </ul>
              <div class="contact__actions">
                <el-button round plain type="primary"><el-icon><Document /></el-icon> One-Pager</el-button>
                <el-button round plain><el-icon><Link /></el-icon> Projektseite</el-button>
              </div>
            </el-card>
          </el-col>
        </el-row>
      </section>

      <!-- FAQ -->
      <section class="section" id="faq">
        <h2>Häufige Fragen</h2>
        <el-collapse accordion>
          <el-collapse-item v-for="(q,i) in faqs" :key="i" :title="q.q">
            <p>{{ q.a }}</p>
          </el-collapse-item>
        </el-collapse>
      </section>
    </el-main>
  </el-container>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import {
  Cpu,
  Compass,
  DocumentAdd,
  Search,
  CollectionTag,
  Lock,
  Message,
  OfficeBuilding,
  Phone,
  Document,
  Link,
} from '@element-plus/icons-vue'
import { useRouter } from 'vue-router'
import useUserStore from '@/stores/user.ts'

const router = useRouter()
const userStore = useUserStore()

// Feature-Kacheln
const features = reactive([
  {
    title: 'Datenkatalog',
    desc: 'Durchsuche, filtere und bewerte Datenquellen aus Verwaltung, Wirtschaft und Forschung.',
    tags: ['Suche', 'Filter', 'Metadaten'],
    icon: Search,
  },
  {
    title: 'Bedarfsmeldungen',
    desc: 'Melde öffentlich, welche Daten du brauchst. Anbieter können direkt andocken.',
    tags: ['Matching', 'Marktplatz'],
    icon: CollectionTag,
  },
  {
    title: 'Zertifikate & Zugang',
    desc: 'Hinterlege Sicherheitsnachweise (z. B. ISO, DPA, Ethik) und beantrage Zugang.',
    tags: ['Prüfprozess', 'Transparenz'],
    icon: Lock,
  },
  {
    title: 'Sicherheit & Rollen',
    desc: 'Rollenbasiertes Berechtigungsmodell mit nachvollziehbarer Protokollierung.',
    tags: ['ABAC', 'Audit'],
    icon: Cpu,
  },
])

// Schritte
const steps = reactive([
  { kicker: '1', title: 'Entdecken & Finden', text: 'Katalog öffnen, Datenquellen filtern und Metadaten prüfen.', icon: Compass },
  { kicker: '2', title: 'Bedarf teilen', text: 'Anwendungsfall beschreiben, Tags setzen – Community sieht’s sofort.', icon: CollectionTag },
  { kicker: '3', title: 'Zertifikate hochladen', text: 'Nachweise hinterlegen und zur Prüfung einreichen.', icon: DocumentAdd },
  { kicker: '4', title: 'Zugang erhalten', text: 'Nach Freigabe sicher & protokolliert auf Daten zugreifen.', icon: Lock },
])

// Stats (Dummy-Werte)
const stats = reactive([
  { label: 'Datenquellen', value: '120+' },
  { label: 'aktive Bedarfsmeldungen', value: '35' },
  { label: 'freigegebene Zugänge', value: '210' },
  { label: 'Organisationen', value: '60+' },
])

// FAQ
const faqs = reactive([
  { q: 'Wer kann den Datenraum nutzen?', a: 'Verwaltung, Unternehmen, Forschung und Zivilgesellschaft – transparent und fair.' },
  { q: 'Wie erhalte ich Zugang zu sensiblen Daten?', a: 'Über Zertifikate/Nachweise. Eine Prüfstelle validiert diese und schaltet frei.' },
  { q: 'Welche Datenlizenzen gibt es?', a: 'Frei, kommunal oder eingeschränkt – je nach Quelle und Schutzbedarf.' },
])

// Newsletter / Kontakt
const email = ref('')
const emailValid = computed(() => /.+@.+\..+/.test(email.value))
const subscribe = () => {
  if (!emailValid.value) {
    ElMessage.error('Bitte eine gültige E-Mail-Adresse eingeben.')
    return
  }
  ElMessage.success('Danke! Wir halten dich auf dem Laufenden.')
  email.value = ''
}

const toLogin = () => {
  if (userStore.isAuthenticated) {
    router.push({ name: 'user-profile' })
  } else {
    router.push({ name: 'login' })
  }
}

// Smooth Scroll
const scrollTo = (id: string) => {
  const el = document.getElementById(id)
  if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

// Hero Hintergrund (dynamischer Gradient via CSS-Var)
const heroGradient = computed(() => ({
  background: 'linear-gradient(135deg, var(--el-color-primary-light-9), #ffffff 40%)',
}))
</script>

<style scoped>
.page { min-height: 100vh; display: flex; flex-direction: column; }
.header { position: sticky; top: 0; z-index: 10; display:flex; align-items:center; justify-content:space-between; backdrop-filter: blur(8px); background: color-mix(in oklab, var(--el-bg-color), white 40%); border-bottom: 1px solid var(--el-border-color-lighter); padding: 0 16px; }
.brand { display:flex; align-items:center; gap:8px; cursor:pointer; }
.logo { transform: rotate(-8deg); }
.nav { display:flex; align-items:center; gap:12px; }
.nav a { color: var(--el-text-color-regular); text-decoration: none; padding: 6px 8px; border-radius: 8px; }
.nav a:hover { background: var(--el-fill-color-lighter); }

.main { padding: 0; }
.hero { display:grid; place-items:center; padding: 72px 16px 48px; }
.hero__content { max-width: 980px; text-align:center; }
.hero__eyebrow { display:flex; gap:8px; align-items:center; justify-content:center; color: var(--el-text-color-secondary); margin-bottom: 8px; }
.hero h1 { font-size: clamp(28px, 4vw, 48px); line-height: 1.1; margin: 8px 0 12px; }
.lede { font-size: 18px; color: var(--el-text-color-regular); }
.hero__cta { margin-top: 16px; display:flex; gap:12px; justify-content:center; flex-wrap:wrap; }
.hero__highlights { display:flex; gap:16px; justify-content:center; flex-wrap:wrap; margin-top: 16px; color: var(--el-text-color-secondary); }
.hero__highlights li { display:flex; gap:6px; align-items:center; }

.section { padding: 48px 16px; max-width: 1120px; margin: 0 auto; }
.section h2 { font-size: 28px; margin-bottom: 16px; }
.feature { height: 100%; border-radius: 16px; }
.feature__icon { width: 44px; height: 44px; display:grid; place-items:center; border-radius: 50%; background: var(--el-fill-color-light); color: var(--el-color-primary); margin-bottom: 8px; }
.feature__tags { margin-top: 8px; display:flex; gap:6px; flex-wrap:wrap; }

.step { display:flex; gap:12px; align-items:flex-start; }
.step__icon { width:40px; height:40px; display:grid; place-items:center; border-radius: 12px; background: var(--el-fill-color); color: var(--el-color-primary); }

.stats { background: var(--el-fill-color-lighter); }
.stat { text-align:center; padding: 16px 8px; }
.stat__value { font-size: 28px; font-weight: 700; }
.stat__label { color: var(--el-text-color-secondary); }

.cta { display:block; }
.newsletter { margin-top: 12px; }
.hint { color: var(--el-text-color-secondary); }
.contact { border-radius: 16px; }
.contact__list { list-style:none; padding:0; margin:12px 0; display:grid; gap:8px; }
.contact__list li { display:flex; gap:8px; align-items:center; color: var(--el-text-color-regular); }
.contact__actions { display:flex; gap:8px; flex-wrap:wrap; }

.footer { border-top: 1px solid var(--el-border-color-lighter); padding: 16px; }
.footer__inner { display:flex; justify-content:space-between; gap:12px; flex-wrap:wrap; max-width:1120px; margin:0 auto; }
.links { display:flex; gap:12px; }

@media (prefers-color-scheme: dark) {
  .hero { background: linear-gradient(135deg, color-mix(in oklab, var(--el-color-primary), black 75%), transparent 40%); }
}
</style>
